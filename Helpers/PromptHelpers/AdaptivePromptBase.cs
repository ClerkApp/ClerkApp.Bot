﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ClerkBot.Helpers.PromptHelpers
{
    public abstract class AdaptivePromptBase<T> : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private readonly PromptValidator<T> _validator;

        protected AdaptivePromptBase(string dialogId, PromptValidator<T> validator = null)
            : base(dialogId)
        {
            _validator = validator;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (!(options is PromptOptions))
            {
                throw new ArgumentOutOfRangeException(nameof(options), "Prompt options are required for Prompt dialogs");
            }

            // Ensure prompts have input hint set
            var opt = (PromptOptions)options;
            if (opt.Prompt != null && string.IsNullOrEmpty(opt.Prompt.InputHint))
            {
                opt.Prompt.InputHint = InputHints.ExpectingInput;
            }

            if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
            {
                opt.RetryPrompt.InputHint = InputHints.ExpectingInput;
            }

            // Initialize prompt state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>();

            // Send initial prompt
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (PromptOptions)state[PersistedOptions], false, cancellationToken).ConfigureAwait(false);

            // Customization starts here for AdaptiveCard Response:
            /* Reason for removing the adaptive card attachments after prompting it to user,
             * from the stat as there is no implicit support for adaptive card attachments.
             * keeping the attachment will cause an exception : Newtonsoft.Json.JsonReaderException: Error reading JArray from JsonReader.
             * Current JsonReader item is not an array: StartObject.
             * Path ‘[‘BotAccessors.DialogState’].DialogStack.$values[0].State.options.Prompt.attachments.$values[0].content.body’.
             */
            if (state[PersistedOptions] is PromptOptions)
            {
                opt.Prompt.Attachments = null;
            }

            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            // Perform base recognition
            var instance = dc.ActiveDialog;
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];

            var recognized = await OnRecognizeAsync(dc.Context, state, options, cancellationToken).ConfigureAwait(false);

            // Validate the return value
            var isValid = false;
            if (_validator != null)
            {
                var promp = Common.CreateInstance<PromptValidatorContext<T>>(dc.Context, recognized, state, options);
                isValid = await _validator(promp, cancellationToken).ConfigureAwait(false);
            }
            else if (recognized.Succeeded)
            {
                isValid = true;
            }

            // Return recognized value or re-prompt
            if (isValid)
            {
                return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
            }

            if (!dc.Context.Responded)
            {
                await OnPromptAsync(dc.Context, state, options, true, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompts are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the prompt receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the prompt prematurely ending we need to implement this method and
            // simply re-prompt the user.
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            await OnPromptAsync(turnContext, state, options, false, cancellationToken).ConfigureAwait(false);
        }
        protected abstract Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken));

        protected abstract Task<PromptRecognizerResult<string>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken));

        protected IMessageActivity AppendChoices(IMessageActivity prompt, string channelId, IList<Choice> choices, ListStyle style, ChoiceFactoryOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get base prompt text (if any)
            var text = prompt != null && !string.IsNullOrEmpty(prompt.Text) ? prompt.Text : string.Empty;

            // Create temporary msg
            IMessageActivity msg = null;
            switch (style)
            {
                case ListStyle.Inline:
                    msg = ChoiceFactory.Inline(choices, text, null, options);
                    break;

                case ListStyle.List:
                    msg = ChoiceFactory.List(choices, text, null, options);
                    break;

                case ListStyle.SuggestedAction:
                    msg = ChoiceFactory.SuggestedAction(choices, text);
                    break;

                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = text;
                    break;
                default:
                    msg = ChoiceFactory.ForChannel(channelId, choices, text, null, options);
                    break;
            }

            // Update prompt with text, actions and attachments
            if (prompt != null)
            {
                // clone the prompt the set in the options (note ActivityEx has Properties so this is the safest mechanism)
                prompt = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(prompt));

                prompt.Text = msg.Text;

                if (msg.SuggestedActions?.Actions != null && msg.SuggestedActions.Actions.Count > 0)
                {
                    prompt.SuggestedActions = msg.SuggestedActions;
                }

                if (msg.Attachments != null && msg.Attachments.Any())
                {
                    if (prompt.Attachments == null)
                    {
                        prompt.Attachments = msg.Attachments;
                    }
                    else
                    {
                        prompt.Attachments = prompt.Attachments.Concat(msg.Attachments).ToList();
                    }
                }

                return prompt;
            }
            else
            {
                msg.InputHint = InputHints.ExpectingInput;
                return msg;
            }
        }
    }
}

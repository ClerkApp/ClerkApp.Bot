using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;

namespace ClerkBot.Helpers.PromptHelpers
{
    public class AdaptiveCardsPrompt: AdaptivePromptBase<string>
    {
        /// <summary>
        /// A dictionary of Default Choices based on <seealso cref="GetSupportedCultures"/>.
        /// Can be replaced by user using the constructor that contains choiceDefaults.
        /// </summary>
        private readonly Dictionary<string, ChoiceFactoryOptions> _choiceDefaults =
            new Dictionary<string, ChoiceFactoryOptions>(
                PromptCultureModels.GetSupportedCultures().ToDictionary(
                    culture => culture.Locale, culture =>
                        new ChoiceFactoryOptions { InlineSeparator = culture.Separator, InlineOr = culture.InlineOr, InlineOrMore = culture.InlineOrMore, IncludeNumbers = true }));

        public string DefaultLocale { get; set; }
        public ListStyle Style { get; set; }
        public ChoiceFactoryOptions ChoiceOptions { get; set; }
        public FindChoicesOptions RecognizerOptions { get; set; }

        public AdaptiveCardsPrompt(string dialogId, PromptValidator<string> validator = null, string defaultLocale = null)
            : base(dialogId, validator)
        {
            Style = ListStyle.Auto;
            DefaultLocale = defaultLocale;
        }

        public AdaptiveCardsPrompt(string dialogId, Dictionary<string, ChoiceFactoryOptions> choiceDefaults, PromptValidator<string> validator = null, string defaultLocale = null)
            : this(dialogId, validator, defaultLocale)
        {
            _choiceDefaults = choiceDefaults;
        }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Format prompt to send
            IMessageActivity prompt;
            IMessageActivity retryPrompt;
            var culture = DetermineCulture(turnContext.Activity);

            var choices = options.Choices ?? new List<Choice>();
            var channelId = turnContext.Activity.ChannelId;
            var choiceOptions = ChoiceOptions ?? _choiceDefaults[culture];
            var choiceStyle = options.Style ?? Style;
            if (isRetry && options.RetryPrompt != null && options.Prompt != null)
            {
                retryPrompt = AppendChoices(options.RetryPrompt, channelId, choices, choiceStyle, choiceOptions);
                prompt = AppendChoices(options.Prompt, channelId, choices, choiceStyle, choiceOptions);

                await turnContext.SendActivityAsync(retryPrompt, cancellationToken).ConfigureAwait(false);
                await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
            }
            else if (isRetry && options.RetryPrompt != null)
            {
                retryPrompt = AppendChoices(options.RetryPrompt, channelId, choices, choiceStyle, choiceOptions);
                await turnContext.SendActivityAsync(retryPrompt, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                prompt = AppendChoices(options.Prompt, channelId, choices, choiceStyle, choiceOptions);
                await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<string>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<string>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                if (!string.IsNullOrEmpty(message.Text))
                {
                    result.Succeeded = true;
                    result.Value = message.Text;
                }
                /*Add handling for Value from adaptive card*/
                else if (message.Value != null)
                {
                    result.Succeeded = true;
                    result.Value = message.Value.ToString();
                }
            }

            return Task.FromResult(result);
        }

        private string DetermineCulture(Activity activity, FindChoicesOptions opt = null)
        {
            var culture = PromptCultureModels.MapToNearestLanguage(activity.Locale ?? opt?.Locale ?? DefaultLocale ?? PromptCultureModels.English.Locale);
            if (string.IsNullOrEmpty(culture) || !_choiceDefaults.ContainsKey(culture))
            {
                culture = PromptCultureModels.English.Locale;
            }

            return culture;
        }
    }
}

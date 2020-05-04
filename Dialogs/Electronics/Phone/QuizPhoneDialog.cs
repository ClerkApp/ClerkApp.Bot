using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class QuizPhoneDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile UserProfile;

        public QuizPhoneDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            Slots = new List<SlotDetails>();

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[]
            {
                InitialStepAsync,
                CameraFeatureAsync,
                SendAsync,
                ProcessResultsAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.ActiveDialog.State["options"] is UserProfile userProfile)
            {
                UserProfile = userProfile;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> SendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            AddDialog(new SlotFillingDialog(Slots));
            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CameraFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Phone for you with all this features: {string.Join(", ", UserProfile.ElectronicsProfile.PhoneProfile.Features.ToArray())}. "),
                cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

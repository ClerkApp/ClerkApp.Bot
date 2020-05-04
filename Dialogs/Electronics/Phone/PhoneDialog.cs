using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Helpers;
using ClerkBot.Models;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class PhoneDialog : ComponentDialog, ISpecificDialog 
    {
        private readonly BotStateService BotStateService;

        public PhoneDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[]
            {
                WelcomeAsync,
                VerifyMissingInfoAsync,
                QuizMoreInfoAsync,
                FinalStepAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new DynamicPhoneDialog(nameof(DynamicPhoneDialog), BotStateService));
            AddDialog(new QuizPhoneDialog(nameof(QuizPhoneDialog), BotStateService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private static async Task<DialogTurnResult> WelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("I can help you choose the best phone to suit your needs!"), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"I believe I've found a perfect {userProfile.ElectronicsProfile.PhoneProfile.BugetRanges.First()}. Just look at these beauties!"),
                cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> VerifyMissingInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (!userProfile.ElectronicsProfile.PhoneProfile.BugetRanges.Any())
            {
                // something is empty
                return await stepContext.BeginDialogAsync(nameof(DynamicPhoneDialog), userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> QuizMoreInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.ElectronicsProfile.PhoneProfile.Features.Any())
            {
                return await stepContext.BeginDialogAsync(nameof(QuizPhoneDialog), userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

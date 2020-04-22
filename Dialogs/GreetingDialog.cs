using System;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Models;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;

        public GreetingDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name
                userProfile.Name = (string)stepContext.Result;

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}. How can I help you today?"), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

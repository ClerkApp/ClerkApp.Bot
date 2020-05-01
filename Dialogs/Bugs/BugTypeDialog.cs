using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Services;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs.Bugs
{
    public class BugTypeDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        public BugTypeDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

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
            AddDialog(new WaterfallDialog($"{nameof(BugTypeDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(BugTypeDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;

            foreach (var entity in entities)
            {
                if (Common.BugTypes.Any(s => s.Equals(entity.Entity, StringComparison.OrdinalIgnoreCase)))
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                        $"Yes! {entity.Entity} is a Bug Type!"), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                        $"No {entity.Entity} is not a Bug Type."), cancellationToken);
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        public RootDialog(BotStateService botStateService, BotServices botServices)
            : base(Common.BuildDialogId())
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
            AddDialog(new GreetingDialog(nameof(GreetingDialog), _botStateService));
            AddDialog(new BugReportDialog(nameof(BugReportDialog), _botStateService));
            AddDialog(new BugTypeDialog(nameof(BugTypeDialog), _botStateService, _botServices));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = Common.BuildDialogId();
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();

            switch (topIntent.intent)
            {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken);
                case "NewBugReportIntent":
                    return await stepContext.BeginDialogAsync(nameof(BugReportDialog), null, cancellationToken);
                case "QueryBugTypeIntent":
                    return await stepContext.BeginDialogAsync(nameof(BugTypeDialog), null, cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean."), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

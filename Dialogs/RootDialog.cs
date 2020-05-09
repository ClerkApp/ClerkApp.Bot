using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Dialogs.Auth;
using ClerkBot.Dialogs.Bugs;
using ClerkBot.Dialogs.Conversations;
using ClerkBot.Dialogs.Electronics;
using ClerkBot.Helpers;
using ClerkBot.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;

namespace ClerkBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly BotServices BotServices;
        private readonly IConfiguration Configuration;

        public RootDialog(IConfiguration configuration, BotStateService botStateService, BotServices botServices)
            : base(Common.BuildDialogId())
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            BotServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[] {
                InitialStepAsync,
                FinalStepAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new BugReportDialog(nameof(BugReportDialog), BotStateService));
            AddDialog(new BugTypeDialog(nameof(BugTypeDialog), BotStateService, BotServices));

            AddDialog(new GreetingDialog(nameof(GreetingDialog), BotStateService));
            AddDialog(new AuthDialog(nameof(AuthDialog), Configuration));
            AddDialog(new ElectronicDialog(nameof(ElectronicDialog), BotStateService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recognizerResult = await BotServices.Dispatch.RecognizeAsync<ClerkLearningService>(stepContext.Context, cancellationToken);
            var (intent, _) = recognizerResult.TopIntent();

            var intentName = intent.ToString().Split(new[] { "Intent" }, StringSplitOptions.None).First();
            var dialog = string.Concat(intentName, "Dialog").TryGetRootDialog();

            if (dialog != null)
            {
                return await stepContext.BeginDialogAsync(dialog, recognizerResult, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean."), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

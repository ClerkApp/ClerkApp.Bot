using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Centvrio.Emoji;
using ClerkBot.Config;
using ClerkBot.Dialogs.Conversations;
using ClerkBot.Dialogs.Electronics;
using ClerkBot.Helpers;
using ClerkBot.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;

namespace ClerkBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly EnvironmentConfig EnvironmentConfig;
        private readonly BotStateService BotStateService;
        private readonly IBotServices BotServices;
        private readonly IElasticSearchClientService ElasticService;

        public RootDialog(
            IOptions<EnvironmentConfig> environmentConfig,
            BotStateService botStateService,
            IBotServices botServices,
            IElasticSearchClientService elasticService)
            : base(Common.BuildDialogId())
        {
            EnvironmentConfig = environmentConfig.Value;
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            BotServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            ElasticService = elasticService ?? throw new ArgumentNullException(nameof(elasticService));

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
            AddDialog(new GreetingDialog(nameof(GreetingDialog), BotStateService));
            AddDialog(new ElectronicDialog(nameof(ElectronicDialog), BotStateService, ElasticService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (EnvironmentConfig.DebuggingMode)
            {
                return await stepContext.BeginDialogAsync(EnvironmentConfig.GenericDialog, 
                    EnvironmentConfig, cancellationToken);
            }

            var recognizerResult = await BotServices.Dispatch.RecognizeAsync<LuisService>(stepContext.Context, cancellationToken);

            if (TryGetDialogFromUserIntent(out var dialog, recognizerResult.TopIntent().intent))
            {
                return await stepContext.BeginDialogAsync(dialog, recognizerResult, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("I'm sorry I don't know what you mean."), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private static bool TryGetDialogFromUserIntent(out string dialog, LuisService.Intent intentResult)
        {
            var intentName = intentResult.ToString().Split(new[] { "Intent" }, StringSplitOptions.None).First();
            dialog = string.Concat(intentName, "Dialog").TryGetRootDialog();

            return dialog != null;
        }

        private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"I always like to say: \"Never stop from trying other things\" {FacePositive.Grinning}"),
                cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

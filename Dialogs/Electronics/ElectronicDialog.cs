using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Dialogs.Electronics.Phone;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs.Electronics
{
    public class ElectronicDialog : ComponentDialog, IRootDialog
    {
        private readonly BotStateService BotStateService;
        private readonly IElasticSearchClientService ElasticService;

        public ElectronicDialog(string dialogId, BotStateService botStateService, IElasticSearchClientService serviceService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            ElasticService = serviceService;
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new MobileDialog(nameof(MobileDialog), BotStateService, ElasticService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.ActiveDialog.State["options"] is ClerkLearningService luisEntities)
            {
                foreach (var entity in luisEntities.Entities.ElectronicType)
                {
                    var intentName = Common.TryParseEnum<ElectronicType>(entity.First(), out _);
                    var dialog = string.Concat(intentName, "Dialog").TryGetSpecificDialog();

                    if (dialog != null)
                    {
                        return await stepContext.BeginDialogAsync(dialog, null, cancellationToken);
                    }

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry, but I can't help you yet finding the best {intentName}."), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
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

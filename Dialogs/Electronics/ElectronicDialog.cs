using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Centvrio.Emoji;
using ClerkBot.Config;
using ClerkBot.Dialogs.Electronics.Phone;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Models.Dialog;
using ClerkBot.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Dialogs.Electronics
{
    public class ElectronicDialog : ComponentDialog, IGenericDialog
    {
        private readonly BotStateService BotStateService;
        private readonly IElasticSearchClientService ElasticService;

        public ElectronicDialog(
            string dialogId,
            BotStateService botStateService,
            IElasticSearchClientService serviceService) : base(dialogId)
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

        public async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogStatus =
                TryGetDialogFromEntityType<ElectronicType>(out var dialog, stepContext.ActiveDialog.State["options"]);

            switch (dialogStatus)
            {
                case DialogIdentifier.identified:
                    return await stepContext.BeginDialogAsync(dialog, null, cancellationToken);
                case DialogIdentifier.unknown:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry {FaceFantasy.Robot} but I can't help you yet find the best {dialog}."), cancellationToken);
                    break;
                case DialogIdentifier.none:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean {FaceNeutral.DowncastWithSweat}"), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static DialogIdentifier TryGetDialogFromEntityType<T>(out string dialog, object stateOptions) where T : struct
        {
            dialog = string.Empty;

            if (stateOptions is EnvironmentConfig debugMode)
            {
                dialog = debugMode.SpecificDialog;
                return DialogIdentifier.identified;
            }

            if (stateOptions is LuisService luisEntities)
            {
                foreach (var entity in luisEntities.Entities.ElectronicType)
                {
                    var intentName = Common.TryParseEnum<T>(entity.First(), out _); 
                    dialog = string.Concat(intentName, "Dialog").TryGetSpecificDialog();

                    if (!string.IsNullOrWhiteSpace(dialog))
                    {
                        return DialogIdentifier.identified;
                    }

                    dialog = intentName;
                    return DialogIdentifier.unknown;
                }
            }

            return DialogIdentifier.none;
        }
    }
}

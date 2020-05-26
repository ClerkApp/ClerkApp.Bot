using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Features;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class QuizMobileDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile UserProfile;

        public QuizMobileDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                GamingFeatureAsync,
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

            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {
                result.TryGetValue(nameof(CameraMobileFeature), out var cameraResult);
                result.TryGetValue(nameof(GameTypeMobileFeature), out var gameTypeResult);

                if (cameraResult != null)
                {
                    var cameraFeature =  JsonConvert.DeserializeObject<CameraMobileFeature>(cameraResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(cameraFeature);
                }
                if (gameTypeResult != null)
                {
                    var gamingFeature = JsonConvert.DeserializeObject<GameTypeMobileFeature>(gameTypeResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(gamingFeature);
                }
            }

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CameraFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Contains(MobileProfile.PhoneFeatures.camera))
            {
                const string dialogId = "CameraFeaturePrompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                const string fileName = "Cards.Mobile.CameraMobile";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(CameraMobileFeature), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from this list")
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> GamingFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Contains(MobileProfile.PhoneFeatures.gaming))
            {
                const string dialogId = "GamingFeaturePrompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                const string fileName = "Cards.Mobile.GameTypeMobile";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(GameTypeMobileFeature), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from this list")
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
        private UserProfile UserProfile;
        private List<SlotDetails> Slots;
        private IDictionary<string, object> State;

        public QuizMobileDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            Slots = new List<SlotDetails>();
            State = new Dictionary<string, object>();

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[]
            {
                CameraFeatureAsync,
                GamingFeatureAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> profileSteps)
        {
            var shuffledSteps = profileSteps.OrderBy(x => Guid.NewGuid()).ToList();
            shuffledSteps.Insert(0, InitialStepAsync);
            shuffledSteps.Add(ProcessResultsAsync);


            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), shuffledSteps));
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            AddDialog(new SlotFillingDialog(ref Slots, ref State));
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = State;
            UserProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (result.Count > 0)
            {
                result.TryGetValue(nameof(CameraMobileFeature), out var cameraResult);
                result.TryGetValue(nameof(GamingMobileFeature), out var gameTypeResult);

                if (cameraResult != null)
                {
                    var cameraFeature =  JsonConvert.DeserializeObject<CameraMobileFeature>(cameraResult.ToString() ?? string.Empty);
                    if (cameraFeature.Action)
                    {
                        UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(cameraFeature);
                    }
                }
                if (gameTypeResult != null)
                {
                    var gamingFeature = JsonConvert.DeserializeObject<GamingMobileFeature>(gameTypeResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(gamingFeature);
                }
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CameraFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Contains(MobileProfile.PhoneFeatures.camera))
            {
                var feature = UserProfile.ElectronicsProfile.MobileProfile.Features.FirstOrDefault(x =>
                    x.GetType().Name.Equals(nameof(CameraMobileFeature)));

                if (feature != null)
                {
                    return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
                }

                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(CameraMobileFeature), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from this list")
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> GamingFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Contains(MobileProfile.PhoneFeatures.gaming))
            {
                var feature = UserProfile.ElectronicsProfile.MobileProfile.Features.FirstOrDefault(x =>
                    x.GetType().Name.Equals(nameof(GamingMobileFeature)));

                if (feature != null)
                {
                    return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
                }

                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(GamingMobileFeature), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from this list")
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }
    }
}

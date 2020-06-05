using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Centvrio.Emoji;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
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
        private readonly string RetryText = $"Please used the submit button {Geometric.BlueCircle} up there {Body.BackhandIndexUp} after selecting";

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
                GamingFeatureAsync,
                AspectFeatureAsync,
                BudgetFeatureAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> profileSteps)
        {
            var shuffledSteps = profileSteps.OrderBy(x => Guid.NewGuid()).ToList();
            shuffledSteps.Insert(0, InitialStepAsync);
            shuffledSteps.Add(ProcessResultsAsync);

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
                result.TryGetValue(nameof(CameraFeatureMobile), out var cameraResult);
                result.TryGetValue(nameof(GamingFeatureMobile), out var gameTypeResult);
                result.TryGetValue(nameof(BudgetFeatureMobile), out var customRangeResult);
                result.TryGetValue(nameof(AspectFeatureMobile), out var aspectResult);

                if (cameraResult != null)
                {
                    var cameraFeature =  JsonConvert.DeserializeObject<CameraFeatureMobile>(cameraResult.ToString() ?? string.Empty);
                    if (cameraFeature.Action)
                    {
                        UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(cameraFeature);
                    }
                }
                if (gameTypeResult != null)
                {
                    var gamingFeature = JsonConvert.DeserializeObject<GamingFeatureMobile>(gameTypeResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(gamingFeature);
                }

                if (customRangeResult != null)
                {
                    var customRange = JsonConvert.DeserializeObject<BudgetFeatureMobile>(customRangeResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(customRange);
                }

                if (aspectResult != null)
                {
                    var aspectFeature = JsonConvert.DeserializeObject<AspectFeatureMobile>(aspectResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.TryAddFeature(aspectFeature);
                }
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AspectFeatureAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.WantedFeatures.Aspect)
            {
                var feature = UserProfile.ElectronicsProfile.MobileProfile.Features.FirstOrDefault(x =>
                    x.GetType().Name.Equals(nameof(AspectFeatureMobile)));

                if (feature != null)
                {
                    return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
                }

                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<AspectFeatureMobile>));

                Slots.Add(new SlotDetails(dialogTypeName.GetCardName(), dialogId, new PromptOptions
                {
                    Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                    RetryPrompt = MessageFactory.Text($"Interesting size {FaceNeutral.Thinking} but can we use the known options {Body.IndexUp} from up there {FaceRole.SmilingWithHalo}")
                }));
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> BudgetFeatureAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.BudgetRanges.Contains(BudgetRanges.custom))
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<BudgetFeatureMobile>));

                Slots.Add(new SlotDetails(dialogTypeName.GetCardName(), dialogId, new PromptOptions
                {
                    Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                    RetryPrompt = MessageFactory.Text($"I'm pretty sure that's not {Money.DollarSign} what I expected {FaceNegative.Dizzy}")
                }));
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> CameraFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.WantedFeatures.Camera)
            {
                var feature = UserProfile.ElectronicsProfile.MobileProfile.Features.FirstOrDefault(x =>
                    x.GetType().Name.Equals(nameof(CameraFeatureMobile)));

                if (feature != null)
                {
                    return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
                }

                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<CameraFeatureMobile>));

                Slots.Add(new SlotDetails(dialogTypeName.GetCardName(), dialogId, new PromptOptions
                {
                    Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                    RetryPrompt = MessageFactory.Text(RetryText)
                }));
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> GamingFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.WantedFeatures.Gaming)
            {
                var feature = UserProfile.ElectronicsProfile.MobileProfile.Features.FirstOrDefault(x =>
                    x.GetType().Name.Equals(nameof(GamingFeatureMobile)));

                if (feature != null)
                {
                    return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
                }

                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<GamingFeatureMobile>));

                Slots.Add(new SlotDetails(dialogTypeName.GetCardName(), dialogId, new PromptOptions
                {
                    Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                    RetryPrompt = MessageFactory.Text(RetryText)
                }));
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private static Task<bool> ObjectDialogValidatorAsync<T>(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (!promptContext.Recognized.Succeeded)
            {
                return Task.FromResult(false);
            }

            try
            {
                JsonConvert.DeserializeObject<T>(promptContext.Recognized.Value);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}

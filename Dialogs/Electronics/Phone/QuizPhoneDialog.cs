using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models.Electronics.Phone;
using ClerkBot.Models.Electronics.Phone.Features;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class QuizPhoneDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile UserProfile;

        public QuizPhoneDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                var cameraFeature =
                    JsonConvert.DeserializeObject<CameraPhoneFeature>(result[nameof(CameraPhoneFeature)].ToString() ?? string.Empty);

                UserProfile.ElectronicsProfile.PhoneProfile.TryAddFeature(cameraFeature);
            }

            //using var test = UserProfile.ElectronicsProfile.PhoneProfile.Features.GetEnumerator();
            //await stepContext.Context.SendActivityAsync(
            //    MessageFactory.Text($"Phone for you with all this features: {string.Join(", ", )}. "),
            //    cancellationToken);

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CameraFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.PhoneProfile.FeaturesList.Contains(PhoneProfile.PhoneFeatures.camera))
            {
                const string dialogId = "CameraFeaturePrompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                const string fileName = "Cards.AdaptiveCards.Phone.ChoiceSet.PhoneCamera";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(CameraPhoneFeature), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from this list")
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private static Task<bool> PhoneFeaturesValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            var findOne = true;
            if (promptContext.Recognized.Succeeded)
            {
                var inputList = promptContext.Recognized.Value.TryGetValues(nameof(PhoneProfile.FeaturesList));
                foreach (var _ in inputList.Select(input => Enum.TryParse(typeof(PhoneProfile.PhoneFeatures), input, out _)).Where(result => findOne && !result))
                {
                    findOne = false;
                }
                valid = findOne;
            }
            return Task.FromResult(valid);
        }
    }
}

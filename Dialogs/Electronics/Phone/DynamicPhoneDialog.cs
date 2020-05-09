using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models.Electronics.Phone;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Choice = Microsoft.Bot.Builder.Dialogs.Choices.Choice;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class DynamicPhoneDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile UserProfile;

        public DynamicPhoneDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                BugetRangeAsync,
                BestFeatureAsync,
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
                var budgetRange = (FoundChoice)result[nameof(PhoneProfile.BugetRanges)];
                var featureRange = result.TryGetChoiceSet(nameof(PhoneProfile.FeaturesList));

                Enum.TryParse(budgetRange.Value, out BugetRanges budgetResult);
                UserProfile.ElectronicsProfile.PhoneProfile.BugetRanges.Add(budgetResult);

                foreach (var choice in featureRange)
                {
                    Common.TryParseEnum(choice, out PhoneProfile.PhoneFeatures featureResult);
                    UserProfile.ElectronicsProfile.PhoneProfile.FeaturesList.Add(featureResult);
                }
            }

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BugetRangeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.PhoneProfile.BugetRanges.Any())
            {
                const string fileName = "Cards.AdaptiveCards.Phone.PostBack.PhoneBugetRange";
                var adaptiveCardContract = JsonConvert.DeserializeObject<AdaptiveCardContract>(new EmbeddedResourceReader(fileName).GetJson());

                var title = adaptiveCardContract.Body.First(x => x.Id.Equals("Prompt")).Text;
                var retry = adaptiveCardContract.Body.First(x => x.Id.Equals("RetryPrompt")).Text;
                var choices = adaptiveCardContract.Body.First(x => x.Id.Equals("Choices")).Choices.Select(choice =>
                    new Choice
                    {
                        Value = choice.Value,
                        Action = new CardAction
                        {
                            Title = choice.Title,
                            Type = ActionTypes.PostBack,
                            Value = choice.Value
                        }
                    }).ToList();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(PhoneProfile.BugetRanges), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(title),
                        RetryPrompt = MessageFactory.Text(retry),
                        Choices = choices
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BestFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.PhoneProfile.FeaturesList.Any())
            {
                const string dialogId = "BestFeaturePrompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, PhoneFeaturesValidatorAsync));

                const string fileName = "Cards.AdaptiveCards.Phone.ChoiceSet.PhoneWantedFeatures";
                var embeddedReader = new EmbeddedResourceReader(fileName);
                var cardAttachment = embeddedReader.CreateAdaptiveCardAttachment();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(PhoneProfile.FeaturesList), dialogId, new SlotPromptOptions(
                        cardAttachment,
                        "Please choose something from this list",
                        embeddedReader.GetJson(),
                        "Input.ChoiceSet"))
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

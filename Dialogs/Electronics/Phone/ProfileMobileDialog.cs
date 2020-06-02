using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models;
using ClerkBot.Models.Dialog;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Choice = Microsoft.Bot.Builder.Dialogs.Choices.Choice;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class ProfileMobileDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private UserProfile UserProfile;
        private List<SlotDetails> Slots;
        private IDictionary<string, object> State;

        public ProfileMobileDialog(
            string dialogId,
            BotStateService botStateService)
            : base(dialogId)
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
                BudgetRangeAsync,
                ReliableBrandsAsync,
                DurabilityAsync,
                BestFeatureAsync,
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
            AddDialog(new WaterfallDialog(Common.BuildDialogId(), shuffledSteps.ToArray()));
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
                result.TryGetValue(nameof(MobileProfile.ReliableBrands), out var reliableResult);
                result.TryGetFoundChoice(nameof(MobileProfile.BudgetRanges), out var budgetRange);
                result.TryGetFoundChoice(nameof(MobileProfile.Durability), out var durability);
                result.TryGetChoiceSet(nameof(MobileProfile.FeaturesList), out var featureRange);

                Enum.TryParse(budgetRange.Value.ToLower(), out BudgetRanges budgetResult);
                UserProfile.ElectronicsProfile.MobileProfile.BudgetRanges.Add(budgetResult);

                Intensity.TryFromName(durability.Value, out var durabilityResult);
                UserProfile.ElectronicsProfile.MobileProfile.Durability = durabilityResult;

                var reliable = JsonConvert.DeserializeObject<CardAction<bool>>(reliableResult.ToString() ?? string.Empty);
                UserProfile.ElectronicsProfile.MobileProfile.ReliableBrands = reliable.Action;

                foreach (var choice in featureRange)
                {
                    Common.TryParseEnum(choice, out MobileProfile.PhoneFeatures featureResult);
                    UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Add(featureResult);
                }
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BudgetRangeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.MobileProfile.BudgetRanges.Any())
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";

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
                    new SlotDetails(nameof(MobileProfile.BudgetRanges), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(title),
                        RetryPrompt = MessageFactory.Text(retry),
                        Choices = choices
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ReliableBrandsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.MobileProfile.ReliableBrands)
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var cardAttachment = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId));

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.ReliableBrands), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please one of the above options")
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DurabilityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.Durability == 0)
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
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
                    new SlotDetails(nameof(MobileProfile.Durability), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(title),
                        RetryPrompt = MessageFactory.Text(retry),
                        Choices = choices
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> BestFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Any())
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var embeddedReader = new EmbeddedResourceReader(fileName);
                var cardAttachment = embeddedReader.CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, PhoneFeaturesValidatorAsync));



                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.FeaturesList), dialogId, new SlotPromptOptions(
                        cardAttachment,
                        "Please choose something from this list",
                        embeddedReader.GetJson(),
                        "Input.ChoiceSet"))
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private static Task<bool> PhoneFeaturesValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            var findOne = true;
            if (promptContext.Recognized.Succeeded)
            {
                var inputList = promptContext.Recognized.Value.TryGetValues(nameof(MobileProfile.FeaturesList));
                foreach (var _ in inputList.Select(input => Enum.TryParse(typeof(MobileProfile.PhoneFeatures), input, out _)).Where(result => findOne && !result))
                {
                    findOne = false;
                }
                valid = findOne;
            }
            return Task.FromResult(valid);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Enums;
using ClerkBot.Helpers;
using ClerkBot.Helpers.DialogHelpers;
using ClerkBot.Helpers.ElasticHelpers;
using ClerkBot.Helpers.PromptHelpers;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Nest;
using Newtonsoft.Json;
using Choice = Microsoft.Bot.Builder.Dialogs.Choices.Choice;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class ProfileMobileDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly ElasticClient ElasticClient;
        private readonly List<SlotDetails> Slots;
        private UserProfile UserProfile;

        public ProfileMobileDialog(
            string dialogId,
            BotStateService botStateService,
            ElasticClient elasticClient)
            : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            ElasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            Slots = new List<SlotDetails>();

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[]
            {
                InitialStepAsync,
                BugetRangeAsync,
                //BrandsAsync,
                DurabilityAsync,
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
                var budgetRange = (FoundChoice)result[nameof(MobileProfile.BugetRanges)];
                var durability = (FoundChoice)result[nameof(MobileProfile.Durability)];
                // var brand = (FoundChoice)result[nameof(MobileProfile.Brands)];
                var featureRange = result.TryGetChoiceSet(nameof(MobileProfile.FeaturesList));

                Enum.TryParse(budgetRange.Value.ToLower(), out BugetRanges budgetResult);
                UserProfile.ElectronicsProfile.MobileProfile.BugetRanges.Add(budgetResult);

                Intensity.TryFromName(durability.Value, out var durabilityResult);
                UserProfile.ElectronicsProfile.MobileProfile.Durability = durabilityResult;

                // UserProfile.ElectronicsProfile.MobileProfile.Brands = new List<string> { brand.Value };

                foreach (var choice in featureRange)
                {
                    Common.TryParseEnum(choice, out MobileProfile.PhoneFeatures featureResult);
                    UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Add(featureResult);
                }
            }

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BrandsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.Brands is null)
            {
                var brandLists = ElasticClient.Search<MobileContract>(s => s
                    .Index("mobiles")
                    .From(0)
                    .Size(0)
                    .Aggregations(new AggregationDictionary
                    {
                        {
                            "brands", new TermsAggregation("Brand")
                            {
                                Field = "Name.Brand.keyword"
                            }
                        }
                    }));

                var searchResult = ElasticProductSearchResultBuilder.BuildProductSearchResult(brandLists);
                searchResult.StringAggregations.TryGetValue("brands", out var aggr);


                if (aggr != null)
                {
                    aggr.Add("None", 0);

                    const string title = "Do you have some prefer mobile brands?";
                    //var retry = adaptiveCardContract.Body.First(x => x.Id.Equals("RetryPrompt")).Text;
                    var choices = aggr.Select(choice =>
                        new Choice
                        {
                            Value = choice.Key,
                            Action = new CardAction
                            {
                                Title = choice.Key,
                                Type = ActionTypes.PostBack,
                                Value = choice.Key
                            }
                        }).ToList();

                    Slots.AddRange(new List<SlotDetails>
                    {
                        new SlotDetails(nameof(MobileProfile.Brands), nameof(ChoicePrompt), new PromptOptions
                        {
                            Prompt = MessageFactory.Text(title),
                            Choices = choices
                        })
                    });
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BugetRangeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.MobileProfile.BugetRanges.Any())
            {
                const string fileName = "Cards.Mobile.BugetRangeMobile";
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
                    new SlotDetails(nameof(MobileProfile.BugetRanges), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(title),
                        RetryPrompt = MessageFactory.Text(retry),
                        Choices = choices
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> DurabilityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.Durability == 0)
            {
                const string fileName = "Cards.Mobile.DurabilityMobile";
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

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BestFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (!UserProfile.ElectronicsProfile.MobileProfile.FeaturesList.Any())
            {
                const string dialogId = "BestFeaturePrompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, PhoneFeaturesValidatorAsync));

                const string fileName = "Cards.Mobile.WantedFeaturesMobile";
                var embeddedReader = new EmbeddedResourceReader(fileName);
                var cardAttachment = embeddedReader.CreateAdaptiveCardAttachment();

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.FeaturesList), dialogId, new SlotPromptOptions(
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

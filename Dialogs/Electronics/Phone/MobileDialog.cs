using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Helpers;
using ClerkBot.Helpers.ElasticHelpers;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.Electronics.Mobile.Enrichment;
using ClerkBot.Models.User;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Nest;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class MobileDialog : ComponentDialog, ISpecificDialog 
    {
        private readonly BotStateService BotStateService;
        private readonly ElasticClient ElasticClient;

        public MobileDialog(string dialogId, BotStateService botStateService, IElasticSearchClientService clientService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            ElasticClient = clientService.GetClient();
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[] {
                WelcomeAsync,
                GetProfileInfoAsync,
                QuizMoreInfoAsync,
                FinalStepAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new ProfileMobileDialog(nameof(ProfileMobileDialog), BotStateService, ElasticClient));
            AddDialog(new QuizMobileDialog(nameof(QuizMobileDialog), BotStateService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        private static async Task<DialogTurnResult> WelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("I can help you choose the best phone to suit your needs!"), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var filterBuilder = new MobileFiltersBuilder<MobileProfile, MobileContract>(userProfile.ElectronicsProfile.MobileProfile);

            var searchResponse =
                ElasticClient.Search<MobileContract>(s => s
                    .Index("mobiles")
                    .From(0)
                    .Size(10)
                    .Query(filterBuilder.BuildQuery())
                    .Sort(filterBuilder.BuildSort()));

            var queryTest = searchResponse.ToJson();
            var searchResult = ElasticProductSearchResultBuilder.BuildProductSearchResult(searchResponse);

            var botResponse = new List<IActivity>();
            if (searchResult.Products.Any())
            {
                var phoneCards = searchResult.Products.Select(mobile => new HeroCard
                    {
                        Title = mobile.Name.Main.ToTitleCase(),
                        Images = new List<CardImage> { new CardImage(mobile.Image.ToString()) },
                        Text = $"- Price: {mobile.Price.First(s => s.Type.Equals(nameof(CurrencyType.EUR))).Value} {nameof(CurrencyType.EUR)}"
                    }.ToAttachment())
                    .ToList();

                botResponse.AddRange(new List<IActivity>
                {
                    MessageFactory.Text($"I believe I've found a list of {userProfile.ElectronicsProfile.MobileProfile.BudgetRanges.First()} phone that perfectly fits you:"),
                    MessageFactory.Carousel(phoneCards)
                });
            }
            else
            {
                botResponse.AddRange(new List<IActivity>
                {
                    MessageFactory.Text(
                        $"I'm sorry, but I don't think there is a phone in this {userProfile.ElectronicsProfile.MobileProfile.BudgetRanges.First()} range that will fulfill your wishes."),
                    MessageFactory.Text("You could try again, maybe increasing your budget or taking out some features...")
                });
            }

            await stepContext.Context.SendActivitiesAsync(botResponse.ToArray(), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> GetProfileInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (!userProfile.ElectronicsProfile.MobileProfile.BudgetRanges.Any())
            {
                return await stepContext.BeginDialogAsync(nameof(ProfileMobileDialog), null, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> QuizMoreInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.ElectronicsProfile.MobileProfile.FeaturesList.Any())
            {
                return await stepContext.BeginDialogAsync(nameof(QuizMobileDialog), userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

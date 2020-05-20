using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Contracts;
using ClerkBot.Helpers;
using ClerkBot.Helpers.ElasticHelpers;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.User;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Nest;

namespace ClerkBot.Dialogs.Electronics.Phone
{
    public class PhoneDialog : ComponentDialog, ISpecificDialog 
    {
        private readonly BotStateService BotStateService;
        private readonly ElasticClient ElasticClient;

        public PhoneDialog(string dialogId, BotStateService botStateService, IElasticSearchClientService clientService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            ElasticClient = clientService.GetClient();
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddActiveDialogs(new WaterfallStep[] {
                WelcomeAsync,
                VerifyMissingInfoAsync,
                QuizMoreInfoAsync,
                FinalStepAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new DynamicPhoneDialog(nameof(DynamicPhoneDialog), BotStateService, ElasticClient));
            AddDialog(new QuizPhoneDialog(nameof(QuizPhoneDialog), BotStateService));

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

            var (minBuget, maxBuget) = new MobileBugetRange()
            {
                Budget = userProfile.ElectronicsProfile.MobileProfile.BugetRanges.First()
            }.GetBudgetRangeInEuro();

            var foundedPhones = ElasticClient.Search<MobileElastic>(s => s
                .Index("mobiles")
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b
                    .Must(m => m
                        .Match(matchQueryDescriptor => matchQueryDescriptor
                            .Field("Name.Brand")
                            .Query(userProfile.ElectronicsProfile.MobileProfile.Brands.First())))) &&
                            q.Range(r => r
                                .Field("Price.EUR")
                                .GreaterThan(minBuget)
                                .LessThan(maxBuget))));

            var searchResult = ElasticProductSearchResultBuilder.BuildProductSearchResult(foundedPhones);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"I believe I've found a perfect {userProfile.ElectronicsProfile.MobileProfile.BugetRanges.First()} " +
                                    $"and {userProfile.ElectronicsProfile.MobileProfile.Durability}. Just look at these beauties!"),
                cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> VerifyMissingInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (!userProfile.ElectronicsProfile.MobileProfile.BugetRanges.Any())
            {
                // something is empty
                return await stepContext.BeginDialogAsync(nameof(DynamicPhoneDialog), userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> QuizMoreInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (userProfile.ElectronicsProfile.MobileProfile.FeaturesList.Any())
            {
                return await stepContext.BeginDialogAsync(nameof(QuizPhoneDialog), userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

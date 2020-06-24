using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Centvrio.Emoji;
using ClerkBot.FilterBuilders.Electronics.Mobile;
using ClerkBot.Helpers;
using ClerkBot.Helpers.ElasticHelpers;
using ClerkBot.Models.Dialog;
using ClerkBot.Models.Electronics.Mobile;
using ClerkBot.Models.User;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Nest;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;

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
                ProfileInfoAsync,
                QuizInfoAsync,
                ResultCardAsync
            });

            InitialDialogId = Common.BuildDialogId();
        }

        private void AddActiveDialogs(IEnumerable<WaterfallStep> waterfallSteps)
        {
            AddDialog(new ProfileMobileDialog(nameof(ProfileMobileDialog), BotStateService));
            AddDialog(new QuizMobileDialog(nameof(QuizMobileDialog), BotStateService));

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
        }

        public async Task<DialogTurnResult> WelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"I can help you choose the best phone {Centvrio.Emoji.Phone.Mobile} to suit your needs {FacePositive.Squinting}"),
                cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> ResultCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var filterBuilder = new BaseBuilderMobile<MobileProfile, MobileContract>(userProfile.ElectronicsProfile.MobileProfile);

            var searchResponse =
                ElasticClient.Search<MobileContract>(s => s
                    .Index("mobiles")
                    .From(0)
                    .Size(10)
                    .TrackScores()
                    .Query(filterBuilder.GetQuery())
                    .Sort(filterBuilder.GetSort()));

            var queryTest = searchResponse.ToJson();
            var searchResult = new ElasticProductSearchResultBuilder<MobileContract>().BuildProductSearchResult(searchResponse);

            var botResponse = new List<IActivity>();
            if (searchResult.Products.Any())
            {
                var phoneCards = new List<Attachment>();

                Parallel.ForEach(searchResult.Products, mobile =>
                {
                    var dialogTypeName = GetType().Name.GetDialogType();
                    var resourceCardName = dialogTypeName.GetCardName();
                    var fileName = $"{dialogTypeName}.{resourceCardName}";

                    var phoneResultTemplate = new AdaptiveCardTemplate(new EmbeddedResourceReader(fileName).GetJson());

                    phoneCards.Add(Common.CreateAdaptiveCardAttachment(phoneResultTemplate.Expand(JsonConvert.SerializeObject(mobile))));
                });

                botResponse.Add(MessageFactory.Text(
                    $"{FaceRole.Partying} I believe I've found a list of {userProfile.ElectronicsProfile.MobileProfile.BudgetRanges.First()} " + 
                    $"phone that perfectly fits you {FacePositive.GrinningSmilingEyes}"));
                phoneCards.ChunkBy(2).ForEach(chunk => 
                    botResponse.AddRange(new List<IActivity>
                    {
                        MessageFactory.Carousel(chunk)
                    }));
            }
            else
            {
                botResponse.AddRange(new List<IActivity>
                {
                    MessageFactory.Text(
                        $"Sorry {FaceNegative.Crying} but I don't think there is a phone {Centvrio.Emoji.Phone.Mobile}" +
                        $"in this {userProfile.ElectronicsProfile.MobileProfile.BudgetRanges.First()} range that will fulfill your wishes {Money.Receipt}"),
                    MessageFactory.Text($"You could try again, maybe increasing {Arrow.Up} your budget or taking out {OtherSymbols.CrossMark} some features...")
                });
            }

            await stepContext.Context.SendActivitiesAsync(botResponse.ToArray(), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> ProfileInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var dialog = nameof(ProfileMobileDialog).TryGetProfileDialog();

            if (dialog != null && !userProfile.ElectronicsProfile.MobileProfile.ArePropertiesNotNull())
            {
                return await stepContext.BeginDialogAsync(dialog, null, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        public async Task<DialogTurnResult> QuizInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var dialog = nameof(QuizMobileDialog).TryGetQuizDialog();

            if (dialog != null && !userProfile.ElectronicsProfile.MobileProfile.WantedFeatures.AreSomePropertiesFalse())
            {
                return await stepContext.BeginDialogAsync(dialog, userProfile, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}

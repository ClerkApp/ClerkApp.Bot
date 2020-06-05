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
                WantedFeatureAsync,
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
                result.TryGetValue(nameof(MobileProfile.WantedFeatures), out var featuresResult);
                result.TryGetFoundChoice(nameof(MobileProfile.BudgetRanges), out var budgetRange);
                result.TryGetFoundChoice(nameof(MobileProfile.Durability), out var durability);

                Enum.TryParse(budgetRange.Value.ToLower(), out BudgetRanges budgetResult);
                UserProfile.ElectronicsProfile.MobileProfile.BudgetRanges.Add(budgetResult);

                Intensity.TryFromName(durability.Value, out var durabilityResult);
                UserProfile.ElectronicsProfile.MobileProfile.Durability = durabilityResult;

                var reliable = JsonConvert.DeserializeObject<CardAction<bool>>(reliableResult.ToString() ?? string.Empty);
                UserProfile.ElectronicsProfile.MobileProfile.ReliableBrands = reliable.Action;

                if (featuresResult != null)
                {
                    var features = JsonConvert.DeserializeObject<MobileProfile.PhoneFeatures>(featuresResult.ToString() ?? string.Empty);
                    UserProfile.ElectronicsProfile.MobileProfile.WantedFeatures = features;
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

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.BudgetRanges), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(adaptiveCardContract.GetTitle()),
                        RetryPrompt = MessageFactory.Text(adaptiveCardContract.GetRetryPrompt()),
                        Choices = adaptiveCardContract.GetChoicesList()
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
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<CardAction<bool>>));

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.ReliableBrands), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please select one of the options")
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

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.Durability), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(adaptiveCardContract.GetTitle()),
                        RetryPrompt = MessageFactory.Text(adaptiveCardContract.GetRetryPrompt()),
                        Choices = adaptiveCardContract.GetChoicesList()
                    })
                });
            }

            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> WantedFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (UserProfile.ElectronicsProfile.MobileProfile.WantedFeatures.AreSomePropertiesFalse())
            {
                var dialogTypeName = GetType().Name.GetDialogType();
                var resourceCardName = dialogTypeName.GetCardName();
                var fileName = $"{dialogTypeName}.{resourceCardName}";
                var embeddedReader = new EmbeddedResourceReader(fileName);
                var cardAttachment = embeddedReader.CreateAdaptiveCardAttachment();

                var dialogId = $"{resourceCardName}Prompt";
                AddDialog(new AdaptiveCardsPrompt(dialogId, ObjectDialogValidatorAsync<MobileProfile.PhoneFeatures>));

                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(MobileProfile.WantedFeatures), dialogId, new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                        RetryPrompt = MessageFactory.Text("Please choose something from that list and press on button")
                    })
                });
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

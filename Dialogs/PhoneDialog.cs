using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Models;
using ClerkBot.Prompts;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace ClerkBot.Dialogs
{
    public class PhoneDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile userProfile;

        public PhoneDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            Slots = new List<SlotDetails>();

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), defaultLocale: Culture.English));

            var waterfallSteps = new WaterfallStep[]
            {
                WelcomeAsync,
                BugetRangeAsync,
                BestFeatureAsync,
                SendAsync,
                ProcessResultsAsync
            };

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));
            InitialDialogId = Common.BuildDialogId();
        }

        private static async Task<DialogTurnResult> WelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("I can help you choose the best phone to suit your needs!"), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BugetRangeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Slots.AddRange(new List<SlotDetails>
            {
                new SlotDetails(nameof(PhoneProfile.BugetRanges), nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("There are four types of range you could choose from:"),
                    RetryPrompt = MessageFactory.Text("Please choose something"),
                    Choices = new List<Choice>
                    {
                        new Choice
                        {
                            Value = "lowBudget",
                            Action = new CardAction
                            {
                                Title = "Low-Budget",
                                DisplayText = "Suitable for someone with a restricted amount of money.",
                                Type = ActionTypes.PostBack,
                                Value = "lowBudget"
                            }
                        },
                        new Choice
                        {
                            Value = "midRange",
                            Action = new CardAction
                            {
                                Title = "Mid-Range",
                                DisplayText = "Mid-range phones are more task-specific.",
                                Type = ActionTypes.PostBack,
                                Value = "midRange"
                            }
                        },
                        new Choice
                        {
                            Value = "highEnd",
                            Action = new CardAction
                            {
                                Title = "High-End",
                                DisplayText = "Most expensive of a range of products.",
                                Type = ActionTypes.PostBack,
                                Value = "highEnd"
                            }
                        },
                        new Choice
                        {
                            Value = "flagship",
                            Action = new CardAction
                            {
                                Title = "Flagship",
                                DisplayText = "The best and most important device produced by a company.",
                                Type = ActionTypes.PostBack,
                                Value = "flagship"
                            }
                        }
                    }
                })
            });
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> BestFeatureAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            AddDialog(new AdaptiveCardsPrompt(nameof(AdaptiveCardsPrompt), PhoneNumberValidatorAsync));

            const string fileName = "Cards.AdaptiveCards.ChoiceSet.PhoneWantedFeatures";
            var cardAttachment = Common.CreateAdaptiveCardAttachment(new EmbeddedResourceReader().GetJson(fileName));

            Slots.AddRange(new List<SlotDetails>
            {
                new SlotDetails(nameof(PhoneProfile.Features), nameof(AdaptiveCardsPrompt), new PromptOptions
                {
                    Prompt = (Activity) MessageFactory.Attachment(cardAttachment),
                    RetryPrompt = MessageFactory.Text("Please choose something from this list")
                })
            });

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> SendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            AddDialog(new SlotFillingDialog(Slots));
            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {

                var budgetRange = (FoundChoice)result[nameof(PhoneProfile.BugetRanges)];
                var featureRange = (FoundChoice)result[nameof(PhoneProfile.Features)];
                Enum.TryParse(budgetRange.Value, out BugetRanges budgetResult);
                Enum.TryParse(featureRange.Value, out Features featureResult);

                userProfile.ElectronicsProfile.PhoneProfile.BugetRanges.Add(budgetResult);
                userProfile.ElectronicsProfile.PhoneProfile.Features.Add(featureResult);
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"I believe I've found a perfect {userProfile.ElectronicsProfile.PhoneProfile.BugetRanges.First()} " +
                                    $"phone for you with {userProfile.ElectronicsProfile.PhoneProfile.Features.First()}. Just look at these bauties!"),
                cancellationToken);

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // PromptValidatorContext<IList<DateTimeResolution>>

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = !promptContext.Recognized.Value.Contains("q");
            }
            return Task.FromResult(valid);
        }
    }
}

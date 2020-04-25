using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Models;
using ClerkBot.Prompts;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace ClerkBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        private readonly BotStateService BotStateService;
        private readonly List<SlotDetails> Slots;
        private UserProfile userProfile;

        public GreetingDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                NameStepAsync,
                ChooseLanguageStepAsync,
                SendAsync,
                ProcessResultsAsync
            };

            AddDialog(new WaterfallDialog(Common.BuildDialogId(), waterfallSteps));

            InitialDialogId = Common.BuildDialogId();
        }

        private async Task<DialogTurnResult> SendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            AddDialog(new SlotFillingDialog(Slots));
            return await stepContext.BeginDialogAsync(nameof(SlotFillingDialog), null, cancellationToken);
        }


        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.FullName.Trim()))
            {
                var fullname_slots = new List<SlotDetails>
                {
                    new SlotDetails(nameof(UserProfile.FirstName), nameof(TextPrompt), "Please enter your first name."),
                    new SlotDetails(nameof(UserProfile.LastName), nameof(TextPrompt), "Please enter your last name."),
                };

                Slots.AddRange(new List<SlotDetails> {
                    new SlotDetails(nameof(UserProfile.FullName), Common.BuildDialogId(nameof(UserProfile.FullName))),
                    new SlotDetails(nameof(UserProfile.Age), nameof(NumberPrompt<int>), "Please enter your age."),
                });

                AddDialog(new SlotFillingDialog(fullname_slots, Common.BuildDialogId(nameof(UserProfile.FullName))));
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ChooseLanguageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userProfile.LanguagePreference))
            {
                Slots.AddRange(new List<SlotDetails>
                {
                    new SlotDetails(nameof(UserProfile.LanguagePreference), nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Choose your language:"),
                        Choices = new List<Choice>
                        {
                            new Choice
                            {
                                Value = "ro",
                                Action = new CardAction { Title = "Romanian", Type = ActionTypes.PostBack, Value = "ro" }
                            },
                            new Choice
                            {
                                Value = "en",
                                Action = new CardAction { Title = "English", Type = ActionTypes.PostBack, Value = "en" }
                            }
                        },
                    })
                });
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userProfile = await BotStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {

                var fullname = result[nameof(UserProfile.FullName)] as IDictionary<string, object>;
                var age = result[nameof(UserProfile.Age)];
                var language = (FoundChoice)result[nameof(UserProfile.LanguagePreference)];

                userProfile.Age = int.Parse(age.ToString() ?? string.Empty);
                userProfile.FirstName = fullname[nameof(UserProfile.FirstName)].ToString();
                userProfile.LastName = fullname[nameof(UserProfile.LastName)].ToString();
                userProfile.LanguagePreference = language.Value;
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Nice to meet {userProfile.FullName}, {userProfile.Age} with lang: {userProfile.LanguagePreference}"),
                cancellationToken);

            Slots.Clear();
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

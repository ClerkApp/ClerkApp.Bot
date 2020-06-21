using System.Collections.Generic;
using System.Linq;
using ClerkBot.Models.Dialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Choice = Microsoft.Bot.Builder.Dialogs.Choices.Choice;

namespace ClerkBot.Helpers.PromptHelpers
{
    /// <summary>
    /// Contains settings to pass to a <see cref="Prompt{T}"/> when the prompt is started.
    /// </summary>
    public class SlotPromptOptions
    {
        public SlotPromptOptions(Attachment attachment, string retryText, string json, string bodyType, ListStyle listStyle = ListStyle.None)
        {
            var adaptiveCardContract = JsonConvert.DeserializeObject<AdaptiveCardContract>(json);

            Prompt = (Activity)MessageFactory.Attachment(attachment);
            RetryPrompt = MessageFactory.Text(retryText);
            Choices = ChoiceFactory.ToChoices(adaptiveCardContract.GetChoicesList().Select(x => x.Value).ToList());
            Style = listStyle;
        }

        public PromptOptions GetPromptOptions()
        {
            return new PromptOptions
            {
                Prompt = Prompt,
                RetryPrompt = RetryPrompt,
                Choices = Choices,
                Style = ListStyle.None
            };
        }

        public Attachment Attachment { get; set; }

        public Activity Prompt { get; set; }

        public Activity RetryPrompt { get; set; }

        public IList<Choice> Choices { get; set; }

        public ListStyle? Style { get; set; }

        public object Validations { get; set; }
    }
}

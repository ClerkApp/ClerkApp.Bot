using ClerkBot.Enums;
using ClerkBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ClerkBot.Helpers.PromptHelpers
{
    public class SlotDetails
    {
        public SlotDetails(string resultIdentifier, string dialogTypeId, string prompt = null, string retryPrompt = null)
            : this(resultIdentifier, dialogTypeId, new PromptOptions
            {
                Prompt = MessageFactory.Text(prompt),
                RetryPrompt = MessageFactory.Text(retryPrompt),
            })
        {
        }

        public SlotDetails(string resultIdentifier, string dialogTypeId, PromptOptions options)
        {
            Name = resultIdentifier;
            DialogId = dialogTypeId;
            Options = options;
        }

        public SlotDetails(string name)
        {
            Name = name;
        }

        public SlotDetails GetTipsPrompts(string tipsCardName)
        {
            var tipsCardAttachment = new EmbeddedResourceReader(tipsCardName).CreateAdaptiveCardAttachment();

            DialogId = nameof(DialogTypes.TipsPrompt);
            Options = new PromptOptions
            {
                Prompt = (Activity) MessageFactory.Attachment(tipsCardAttachment)
            };

            return this;
        }

        public string Name { get; set; }

        public string DialogId { get; set; }

        public PromptOptions Options { get; set; }
    }
}

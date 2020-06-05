using System.Collections.Generic;
using ClerkBot.Enums;
using ClerkBot.Models.Dialog;
using ClerkBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ClerkBot.Helpers.PromptHelpers
{
    public class GenerateDialogDetails
    {
        private readonly List<SlotDetails> Dialogs;
        private readonly string ResultIdentifier;
        private readonly string DialogTypeId;
        private readonly string ResourceType;
        private readonly string ResourceCardName;

        public GenerateDialogDetails(string resultIdentifier, string dialogTypeId, string resourceType, string resourceCardName)
        {
            ResultIdentifier = resultIdentifier;
            DialogTypeId = dialogTypeId;
            ResourceType = resourceType;
            ResourceCardName = resourceCardName;
            Dialogs = new List<SlotDetails>();
        }

        public List<SlotDetails> TryGetDialog()
        {
            var resourceCardLocation = $"{ResourceType}.{ResourceCardName}";
            var adaptiveCardContract = JsonConvert.DeserializeObject<AdaptiveCardContract>(new EmbeddedResourceReader(resourceCardLocation).GetJson());

            if (adaptiveCardContract != null)
            {
                Dialogs.Add(new SlotDetails(ResultIdentifier, DialogTypeId,
                    new PromptOptions
                    {
                        RetryPrompt = MessageFactory.Text(adaptiveCardContract.GetRetryPrompt()),
                        Choices = adaptiveCardContract.GetChoicesList()
                    }));
            }

            return Dialogs;
        }

        public List<SlotDetails> TryGetDialogWithTips()
        {
            var resourceCardLocation = $"{ResourceType}.Tips.{ResourceCardName}";
            var tipsCardAttachment = new EmbeddedResourceReader(resourceCardLocation).CreateAdaptiveCardAttachment();

            if (tipsCardAttachment != null)
            {
                Dialogs.Add(new SlotDetails(ResultIdentifier, nameof(DialogTypes.TipsPrompt),
                    new PromptOptions
                    {
                        Prompt = (Activity) MessageFactory.Attachment(tipsCardAttachment)
                    }));
            }

            return TryGetDialog();
        }
    }
}

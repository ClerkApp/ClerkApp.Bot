using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ClerkBot.Models.Dialog
{ 
    public class AdaptiveCardContract
    {
        [JsonProperty("body")]
        private List<Body> Body { get; set; }

        [JsonProperty("actions")]
        private List<Action> Actions { get; set; }

        public string GetTitle()
        {
            return Body.First(x => x.Id.Equals("Prompt")).Text;
        }

        public string GetRetryPrompt()
        {
            return Body.First(x => x.Id.Equals("RetryPrompt")).Text;
        }

        public List<Microsoft.Bot.Builder.Dialogs.Choices.Choice> GetChoicesList()
        {
            return Body.First(x => x.Id.Equals("Choices")).Choices.Select(choice =>
                new Microsoft.Bot.Builder.Dialogs.Choices.Choice
                {
                    Value = choice.Value,
                    Action = new CardAction
                    {
                        Title = choice.Title,
                        Type = ActionTypes.PostBack,
                        Value = choice.Value
                    }
                }).ToList();
        }
    }

    public class Action
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class Body
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isMultiSelect")]
        public bool? IsMultiSelect { get; set; }

        [JsonProperty("value")]
        public long? Value { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClerkBot.Contracts
{ 
    public partial class AdaptiveCardContract
    {
        [JsonProperty("body")]
        public List<Body> Body { get; set; }

        [JsonProperty("actions")]
        public List<Action> Actions { get; set; }
    }

    public partial class Action
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class Body
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

    public partial class Choice
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
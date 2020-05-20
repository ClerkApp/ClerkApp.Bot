using System;
using Bot.Storage.Elasticsearch;
using ClerkBot.Config;
using ClerkBot.Models;
using ClerkBot.Models.User;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;

namespace ClerkBot.Services
{
    public class BotStateService
    {
        // State Variables
        public ConversationState ConversationState { get; }
        public UserState UserState { get; }


        // IDs
        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";
        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";


        // Accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public BotStateService(IOptions<ElasticConfig> elasticConfig)
        {
            ConversationState = new ConversationState(
                new ElasticsearchStorage(
                    new ElasticsearchStorageOptions
                    {
                        ElasticsearchEndpoint = new Uri(elasticConfig.Value.Host),
                        IndexName = elasticConfig.Value.Indexs.Conversations
                    }
                ));

            UserState = new UserState(
                new ElasticsearchStorage(
                    new ElasticsearchStorageOptions
                    {
                        ElasticsearchEndpoint = new Uri(elasticConfig.Value.Host),
                        IndexName = elasticConfig.Value.Indexs.Users
                    }
                ));

            InitializeAccessors();
        }


        public void InitializeAccessors()
        {
            // Initialize Conversation State Accessors
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);

            // Initialize User State
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
        }
    }
}

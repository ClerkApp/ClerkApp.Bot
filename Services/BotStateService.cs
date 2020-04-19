using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClerkBot.Models;
using Microsoft.Bot.Builder;

namespace ClerkBot.Services
{
    public class BotStateService
    {
        // State Variables
        public ConversationState ConversationState { get; set; }
        public UserState UserState { get; set; }

        // IDs
        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationStateId { get; } = $"{nameof(BotStateService)}.ConversationData";

        // Accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public BotStateService(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));

            InitializeAccesors();
        }

        private void InitializeAccesors()
        {
            // Init Convesation State
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationStateId);

            // Init User State
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
        }
    }
}

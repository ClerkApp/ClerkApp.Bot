using System.Threading;
using Microsoft.Bot.Builder;

namespace ClerkBot.Models
{
    public class CacheUser
    {
        public string UserName { get; set; }
        public string BotClientUserId { get; set; }
        public string BotConversationId { get; set; }
        public ITurnContext TurnContext { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool LoginDetected { get; set; }
        public CacheUser(string botClientUserId, string botConversationId, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            BotClientUserId = botClientUserId;
            BotConversationId = botConversationId;
            UserName = "";
            TurnContext = turnContext;
            CancellationToken = cancellationToken;
            LoginDetected = false;
        }
    }
}

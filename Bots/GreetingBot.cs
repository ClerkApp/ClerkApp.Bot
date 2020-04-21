using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Models;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ClerkBot.Bots
{
    public class GreetingBot : ActivityHandler
    {
        private readonly BotStateService _botStateService; 

        public GreetingBot(BotStateService botStateService)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
            ConversationData conversationData = await _botStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData(), cancellationToken);
            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Hi {userProfile.Name}. How can I help you today?"), cancellationToken);
            }
            else
            {
                if (conversationData.PromptedUserForName)
                {
                    // Set the name to what the user provided
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    // Acknowledge that we got their name.
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"Thanks {userProfile.Name}. How can I help you today?"), cancellationToken);

                    // Reset the flag to allow the bot to go though the cycle again.
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    // Prompt the user for their name.
                    await turnContext.SendActivityAsync(MessageFactory.Text($"What is your name?"), cancellationToken);

                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.PromptedUserForName = true;
                }

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);
                await _botStateService.ConversationDataAccessor.SetAsync(turnContext, conversationData, cancellationToken);

                await _botStateService.UserState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
                await _botStateService.ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
            }
        }

    }
}

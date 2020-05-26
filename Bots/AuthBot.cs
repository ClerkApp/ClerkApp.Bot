// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Helpers;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ClerkBot.Bots
{
    public class AuthBot<T> : DialogBot<T> where T : Dialog
    {
        public AuthBot(BotStateService botStateService, T dialog, ILogger<DialogBot<T>> logger)
            : base(botStateService, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendGreetingToAcquaintAsync(turnContext, cancellationToken);
        }

        private async Task SendGreetingToAcquaintAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    const string fileName = "Cards.WelcomeCard";
                    var welcomeCard = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment("Welcome");
                    var response = MessageFactory.Attachment(welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }
    }
}

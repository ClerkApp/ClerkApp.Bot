﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Centvrio.Emoji;
using ClerkBot.Helpers;
using ClerkBot.Resources;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ClerkBot.Bots
{
    public class RootBot<T> : DialogBot<T> where T : Dialog
    {
        public RootBot(BotStateService botStateService, T dialog, ILogger<DialogBot<T>> logger)
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
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    continue;
                }

                const string fileName = "WelcomeCard";
                var welcomeCard = new EmbeddedResourceReader(fileName).CreateAdaptiveCardAttachment("Welcome");
                var dialogNames = Common.TryGetAllSpecificDialog().Select(dialog => dialog.Replace("Dialog", string.Empty)).ToList();
                var options = string.Join(",", dialogNames);
                var botResponse = new List<IActivity>();

                botResponse.AddRange(new List<IActivity>
                {
                    MessageFactory.Attachment(welcomeCard),
                    MessageFactory.Text($"For now {Time.HourglassNotDone} I can help you find: {options}")
                });

                await turnContext.SendActivitiesAsync(botResponse.ToArray(), cancellationToken);
            }
        }
    }
}

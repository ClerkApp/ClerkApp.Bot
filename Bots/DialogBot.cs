using System;
using System.Threading;
using System.Threading.Tasks;
using ClerkBot.Models.Database;
using ClerkBot.Models.User;
using ClerkBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ClerkBot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotStateService BotStateService;
        protected readonly Dialog Dialog;
        protected readonly ILogger Logger;

        public DialogBot(BotStateService botStateService, T dialog, ILogger logger)
        {
            BotStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await BotStateService.UserState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
            await BotStateService.ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }
        
        protected override async Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
        {
            var userProfile = await BotStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
            var conversationData = await BotStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData(), cancellationToken);

            await BotStateService.UserProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);
            await BotStateService.ConversationDataAccessor.SetAsync(turnContext, conversationData, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");
            await Dialog.RunAsync(turnContext, BotStateService.DialogStateAccessor, cancellationToken);
        }

        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Token Response Event Activity.");
            await Dialog.RunAsync(turnContext, BotStateService.DialogStateAccessor, cancellationToken);
        }
    }
}

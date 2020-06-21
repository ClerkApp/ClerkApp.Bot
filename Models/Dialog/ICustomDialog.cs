using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ClerkBot.Models.Dialog
{
    public interface IGenericDialog
    {
        Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
    }

    public interface ISpecificDialog
    {
        Task<DialogTurnResult> WelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> ProfileInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> QuizInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> ResultCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
    }

    public interface IProfileDialog
    {
        Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
    }

    public interface IQuizDialog
    {
        Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken);
    }
}

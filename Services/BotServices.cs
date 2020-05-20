using ClerkBot.Config;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Options;
using LuisPredictionOptions = Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions;

namespace ClerkBot.Services
{
    public class BotServices: IBotServices
    {
        public LuisRecognizer Dispatch { get; }

        public BotServices(IOptions<LuisConfig> luisConfig)
        {
            var luisConfig1 = luisConfig.Value;

            var luisApplication = new LuisApplication(luisConfig1.AppId, luisConfig1.PrimaryKey, luisConfig1.Host);
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                IncludeAPIResults = true,
                PredictionOptions = new LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };

            Dispatch = new LuisRecognizer(recognizerOptions);
        }
    }
}

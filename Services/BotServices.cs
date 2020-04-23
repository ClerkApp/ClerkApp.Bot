using Microsoft.Bot.Builder.AI.Luis;
//using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using LuisPredictionOptions = Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions;

namespace ClerkBot.Services
{
    public class BotServices: IBotServices
    {
        public LuisRecognizer Dispatch { get; }
        //public QnAMaker SampleQnA { get; private set; }

        public BotServices(IConfiguration configuration)
        {
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult
            var luisApplication = new LuisApplication(configuration["ConnectionStrings:Luis:AppId"],
                                             configuration["ConnectionStrings:Luis:PrimaryKey"],
                                             configuration["ConnectionStrings:Luis:Endpoint"]);

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
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

            //SampleQnA = new QnAMaker(new QnAMakerEndpoint
            //{
            //    KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
            //    EndpointKey = configuration["QnAEndpointKey"],
            //    Host = configuration["QnAEndpointHostName"]
            //});
        }
    }
}

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;

namespace TABot.Services.BotServices
{
    public class BotServices : IBotServices
    {
        public BotServices(IConfiguration configuration)
        {
            Dispatch = new LuisRecognizer(
                new LuisApplication(
                    configuration["DispatchLuisAppId"],
                    configuration["DispatchLuisAPIKey"],
                    configuration["DispatchLuisAPIHostName"]), 
                new LuisPredictionOptions {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                },
                true);

            ErrorLuis = new LuisRecognizer(
                new LuisApplication(
                    configuration["ErrorLuisAppId"],
                    configuration["ErrorLuisAPIKey"],
                    configuration["ErrorLuisAPIHostName"]),
                new LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                },
                true);

            QnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAEndpointKey"],
                Host = configuration["QnAEndpointHostName"]
            });
        }
        public LuisRecognizer Dispatch { get; private set; }

        public QnAMaker QnA { get; private set; }

        public LuisRecognizer ErrorLuis { get; private set; }
    }
}

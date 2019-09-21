using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace TABot.Services.BotServices
{
    public interface IBotServices
    {
        LuisRecognizer Dispatch { get; }
        QnAMaker QnA { get; }
        LuisRecognizer ErrorLuis { get; }
    }
}

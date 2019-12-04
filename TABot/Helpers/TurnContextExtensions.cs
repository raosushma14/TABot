using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace TABot.Helpers
{
    public static class TurnContextExtensions
    {
        public static Task<ResourceResponse> ReplyTextAsync(this ITurnContext turnContext, string text, CancellationToken cancellationToken= default(CancellationToken))
        {
            return turnContext.SendActivityAsync(MessageFactory.Text(text));
        }
    }
}

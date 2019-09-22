using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TABot.Helpers;

namespace TABot.Bots.Dialogs
{
    public abstract class LuisDialog : ComponentDialog
    {
        public LuisDialog(string dialogId) : base(dialogId)
        {

        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            await turnContext.ReplyTextAsync("Is there anything else that I can help with?");
            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }
    }
}

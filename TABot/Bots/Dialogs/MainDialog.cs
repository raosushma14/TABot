using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TABot.Services.BotServices;
using TABot.Helpers;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Schema;

namespace TABot.Bots.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private IBotServices _botServices;
        private ILogger<EchoBot> _logger;

        public MainDialog(IBotServices botServices, 
            ILogger<EchoBot> logger,
            ErrorEnquiryDialog errorEnquiryDialog) : base(nameof(MainDialog))
        {
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(errorEnquiryDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WaitForUsersMessage,
                BeginDispatchAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default)
        {
            if (outerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = outerDc.Context.Activity.Text?.ToLowerInvariant();

                switch (text)
                {
                    case "quit":
                    case "cancel":
                        await outerDc.Context.ReplyTextAsync("Quitting the dialog!");
                        return await outerDc.CancelAllDialogsAsync(cancellationToken);

                }
            }

            return await base.ContinueDialogAsync(outerDc, cancellationToken);
        }

        // Step #1
        private async Task<DialogTurnResult> WaitForUsersMessage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(stepContext.Context.Activity.Text, cancellationToken);
        }

        // Step #2
        private async Task<DialogTurnResult> BeginDispatchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var usersMessage = (string) stepContext.Result;

            var dispatchResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = dispatchResult.GetTopScoringIntent();

            return await DispatchToTopIntentAsync(stepContext, topIntent.intent, dispatchResult, cancellationToken);
        }

        private async Task<DialogTurnResult> DispatchToTopIntentAsync(WaterfallStepContext stepContext, string intent, RecognizerResult dispatchResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "l_TALuis":
                    return await ProcessLuisAsync(stepContext, dispatchResult.Properties["luisResult"] as LuisResult, cancellationToken);
                case "q_TAKB":
                     return await ProcessQnAAsync(stepContext, cancellationToken);
                default:
                    await stepContext.Context.ReplyTextAsync("Sorry! I am unable to help with that at the moment.");
                    return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessLuisAsync(WaterfallStepContext stepContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            var result = luisResult.ConnectedServiceResult;
            var topIntent = result.TopScoringIntent.Intent;
            switch (topIntent)
            {
                case "ErrorQuestions":
                    return await stepContext.BeginDialogAsync(nameof(ErrorEnquiryDialog), null, cancellationToken);
                    break;
                case "AssignmentQuestions":
                    await stepContext.Context.ReplyTextAsync("i can help with your assignment");
                    break;
                default:
                    await stepContext.Context.ReplyTextAsync("Sorry! I am unable to understand that");
                    break;
            }
            
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessQnAAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var answers = await _botServices.QnA.GetAnswersAsync(stepContext.Context);

            if (answers.Any())
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(answers.First().Answer), cancellationToken);
            }
            else
            {
                await stepContext.Context.ReplyTextAsync("Sorry! I am unable to answer that at the moment.");
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Step #3
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using TABot.Services.BotServices;
using System.Linq;
using TABot.Helpers;

namespace TABot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private IBotServices _botServices;
        private ILogger<EchoBot> _logger;

        public EchoBot(IBotServices botServices, ILogger<EchoBot> logger)
        {
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var dispatchResult = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
            var topIntent = dispatchResult.GetTopScoringIntent();

            await DispatchToTopIntentAsync(turnContext, topIntent.intent, dispatchResult, cancellationToken);
        }

        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, RecognizerResult dispatchResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "l_TALuis":
                    await ProcessLuisAsync(turnContext, dispatchResult.Properties["luisResult"] as LuisResult, cancellationToken);
                    break;
                case "q_TAKB":
                    await ProcessQnAAsync(turnContext, cancellationToken);
                    break;
                default:
                    await turnContext.ReplyTextAsync("Sorry! I am unable to help with that at the moment.");
                    break;
            }
        }

        private async Task ProcessQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var answers = await _botServices.QnA.GetAnswersAsync(turnContext);

            if (answers.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(answers.First().Answer),cancellationToken);
            }
            else
            {
                await turnContext.ReplyTextAsync("Sorry! I am unable to answer that at the moment.");
            }
       
        }

        private async Task ProcessLuisAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            var result = luisResult.ConnectedServiceResult;
            var topIntent = result.TopScoringIntent.Intent;

            switch (topIntent)
            {
                case "ErrorQuestions":
                    await turnContext.ReplyTextAsync("I can help with error");
                    break;
                case "AssignmentQuestions":
                    await turnContext.ReplyTextAsync("i can help with your assignment");
                    break;
                default:
                    await turnContext.ReplyTextAsync("Sorry! I am unable to understand that");
                    break;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}

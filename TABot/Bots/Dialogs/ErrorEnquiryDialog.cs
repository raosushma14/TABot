using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using TABot.Helpers;

namespace TABot.Bots.Dialogs
{
    public class ErrorEnquiryDialog : ComponentDialog
    {
        private string _initDialogId = "EEDBegin";
        private const string uploadChoice = "Upload a screenshot", textChoice = "Copy & Paste Error message";

        public ErrorEnquiryDialog() : base(nameof(ErrorEnquiryDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                AskErrorTextOrImage,
                SendAppropriatePrompt,
                ProcessWithLuisAndRespond
            };

            AddDialog(new WaterfallDialog(_initDialogId, waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = _initDialogId;
        }

        private async Task<DialogTurnResult> AskErrorTextOrImage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to copy & paste the error message or upload a screenshot of the error?"),
                    Choices = ChoiceFactory.ToChoices(new List<string>
                    {
                        textChoice,
                        uploadChoice
                    }),
                    Style = ListStyle.SuggestedAction
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> SendAppropriatePrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (string) stepContext.Result;

            stepContext.Values["ErrorEntryMethod"] = choice;

            switch (choice)
            {
                case uploadChoice:
                    return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please click the upload button and upload the screenshot")
                    });

                case textChoice:
                default:
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please Copy & Paste the error message in the text box and hit enter")
                    });
            }
        }

        private async Task<DialogTurnResult> ProcessWithLuisAndRespond(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Values["ErrorEntryMethod"];

            switch (choice)
            {
                case textChoice:
                    await stepContext.Context.ReplyTextAsync("Recieved error as text");
                    break;
                case uploadChoice:
                    await stepContext.Context.ReplyTextAsync("Recieved error as image");
                    break;
            }

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}

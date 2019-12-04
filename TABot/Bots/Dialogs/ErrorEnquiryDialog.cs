using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using TABot.Helpers;
using TABot.Services.BotServices;
using TABot.Services.EmailServices;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;

namespace TABot.Bots.Dialogs
{
    public class ErrorEnquiryDialog : LuisDialog
    {
        private const string uploadChoice = "Upload a screenshot", textChoice = "Copy & Paste Error message";

        private IBotServices _botServices;
        private ILogger<EchoBot> _logger;
        private EmailService _emailService;
        private ComputerVisionClient _computerVisionClient;
        
        public ErrorEnquiryDialog(IBotServices botServices, ILogger<EchoBot> logger, 
            EmailService emailService, ComputerVisionClient computerVisionClient) : base(nameof(ErrorEnquiryDialog))
        {
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _computerVisionClient = computerVisionClient ?? throw new ArgumentNullException(nameof(computerVisionClient));

            var waterfallSteps = new WaterfallStep[]
            {
                AskErrorTextOrImage,
                SendAppropriatePrompt,
                ProcessWithLuisAndRespond
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
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
            var choice = ((FoundChoice)stepContext.Result).Value;

            stepContext.Values["ErrorEntryMethod"] = choice;

            switch (choice)
            {
                case uploadChoice:
                    return await stepContext.PromptAsync(nameof(AttachmentPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please click the upload button and upload the screenshot"),
                        RetryPrompt = MessageFactory.Text("Please click the upload button on the bottom left and upload the screenshot. You can also say 'quit' to exit from this dialog."),
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

            bool worked = false;

            List<string> lines = new List<string>();       

            switch (choice)
            {
                case textChoice:
                    {
                        string text = (string)stepContext.Result;

                        lines.AddRange(text.Split('\n'));
                    }
                    break;
                case uploadChoice:
                    var file = ((IEnumerable<Attachment>)stepContext.Result).FirstOrDefault();
                    if(file != null)
                    {
                        using (WebClient client = new WebClient())
                        {
                            var data = client.DownloadData(new Uri(file.ContentUrl));
                            using(MemoryStream stream = new MemoryStream(data))
                            {
                                var response = await _computerVisionClient.RecognizePrintedTextInStreamAsync(false, stream);
                                
                                foreach (var region in response.Regions)
                                {
                                    foreach (var line in region.Lines)
                                    {
                                        StringBuilder builder = new StringBuilder();
                                        foreach (var word in line.Words)
                                        {
                                            builder.Append($"{word.Text} ");
                                        }
                                        string text = builder.ToString();

                                        lines.Add(text);
                                    }
                                }
                            }
                        } 
                    }
                    else
                    {
                        await stepContext.Context.ReplyTextAsync("Sorry, attachment upload failed");
                    }
                    
                    break;
            }

            foreach (var line in lines)
            {
                stepContext.Context.Activity.Text = line;
                var result = await _botServices.ErrorLuis.RecognizeAsync(stepContext.Context, cancellationToken);

                var topIntent = result.GetTopScoringIntent();

                switch (topIntent.intent)
                {
                    case "SegmentationFault":
                        worked = true;
                        var message = "Your program is trying to access a memory that is not allocated for it."; 
                        await stepContext.Context.ReplyTextAsync($"Line - \" {line} \" \n\n{message}");
                        break;
                }
            }

            if (worked)
            {
                await stepContext.Context.ReplyTextAsync("I hope this should give you enough ammunition to fix your code");
            }
            else
            {
                await stepContext.Context.ReplyTextAsync("I was unable to find out what's wrong. I'll let the course instructor know about this.");
            }

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}

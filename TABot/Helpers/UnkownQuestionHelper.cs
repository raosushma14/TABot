using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using TABot.Services.EmailServices;
using TABot.Services.TableStorageService;

namespace TABot.Helpers
{
    public class UnkownQuestionHelper
    {
        public static async Task HandleAsync(ITurnContext context, EmailService emailService)
        {
            var subject = "TABot : I was unable to answer";
            var body = $"Hello Professor, I was unable to answer the question : {context.Activity.Text} asked by the student " +
                $"{context.Activity.From.Name}. And he/she wants to reach out to you";
            var to = "raosushma14@gmail.com";
            var cc = new string[]
            {
                        context.Activity.From.Id
            };

            await emailService.SendEmailAsync(subject, body, to, cc);
        }

        public static async Task LogToTableStorage(ITurnContext context, TableStorageService storageService)
        {
            await storageService.InsertRecordAsync(new Models.LogEntity(
                userId: context.Activity.From.Id,
                userName: context.Activity.From.Name,
                message: context.Activity.Text));
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TABot.Models;

namespace TABot.Services.EmailServices
{
    public class EmailService
    {
        private const string RouteTemplate = "/v3/mail/send";
        private string _baseUrl { get; set; }

        private string _authKey { get; set; }
        public EmailService(string baseUrl, string authKey)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

            _authKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authKey);
            var url = _baseUrl + RouteTemplate;

            SendGridEmailModel model = new SendGridEmailModel
            {
                Personalizations = new List<Recipient>
                {
                    new Recipient
                    {
                        To = new List<EmailAddress>
                        {
                            new EmailAddress{Email = to}
                        }
                    }
                },
                From = new EmailAddress { Email = "raosushma14@gmail.com" },
                Subject = subject,
                Content = new List<EmailContent>
                {
                    new EmailContent
                    {
                        Type = isHtml? "text/html": "text/plain",
                        Value = body
                    }
                }
            };

            string json = JsonConvert.SerializeObject(model);
            var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("Email service api call failed");
            }
        }
    }
}

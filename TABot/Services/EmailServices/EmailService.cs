using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TABot.Models;
using System.Linq;

namespace TABot.Services.EmailServices
{
    public class EmailService
    {
        private const string RouteTemplate = "/v3/mail/send";
        private string _baseUrl { get; set; }
        private string _fromEmail { get; set; }
        private string _toEmail { get; set; }

        private string _authKey { get; set; }

        public EmailService(string baseUrl, string fromEmail, string toEmail, string authKey)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
            _toEmail = toEmail ?? throw new ArgumentNullException(nameof(toEmail));
            _authKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
        }

        public async Task SendEmailAsync(string subject, string body, string to = null, IEnumerable<string> cc = null, bool isHtml = false)
        {
            if(to == null)
            {
                to = _toEmail;
            }

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
                        },
                        Cc = cc?.Select(p=> new EmailAddress
                        {
                            Email = p
                        })
                    }
                },
                From = new EmailAddress { Email = _fromEmail },
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

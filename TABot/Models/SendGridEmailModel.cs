using Newtonsoft.Json;
using System.Collections.Generic;

namespace TABot.Models
{
    public class SendGridEmailModel
    {
        [JsonProperty("personalizations")]
        public IEnumerable<Recipient> Personalizations { get; set; }

        [JsonProperty("from")]
        public EmailAddress From { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("content")]
        public IEnumerable<EmailContent> Content { get; set; }
    }

    public class EmailAddress
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class Recipient
    {
        [JsonProperty("to")]
        public IEnumerable<EmailAddress> To { get; set; }
    }

    public class EmailContent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}

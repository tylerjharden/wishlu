using Newtonsoft.Json;
using System;

namespace Squid.Housekeeping
{
    public enum EmailNotification
    {
        None = -1,
        Day1 = 1,
        Day2 = 2,
        Day3 = 3,
        Day5 = 5,
        Day8 = 8,
        Day13 = 13, 
        Reveal = 14
    }

    // Fantine email task
    public class EmailTask
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }
        [JsonProperty("EmailNotification")]
        public EmailNotification EmailNotification { get; set; }

        [JsonProperty("To")]
        public string To { get; set; }
        [JsonProperty("Body")]
        public string Body { get; set; }
        [JsonProperty("Subject")]
        public string Subject { get; set; }

        public EmailTask()
        {
            EmailNotification = EmailNotification.None;
        }
    }

    // Fantine promise/gift reveal task
    public class RevealTask
    {
        [JsonProperty("PromiseId")]
        public Guid PromiseId { get; set; }
        [JsonProperty("Date")]
        public DateTimeOffset Date { get; set; }
    }

    // Fantine password reset email task
    public class PasswordResetTask
    {
        [JsonProperty("ResetCode")]
        public Guid ResetCode { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }
    }
}

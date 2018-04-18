namespace FeedbackBotAzureFunctions
{
    using System.Collections.Specialized;

    public class MessageReceived
    {
        public string CallSid { get; set; }
        public string SpeechResult { get; set; }
        public string Digits { get; set; }
        public string To { get; set; }

        public MessageReceived()
        {
        }

        public MessageReceived(NameValueCollection col)
        {
            this.CallSid = col.Get(nameof(this.CallSid));
            this.SpeechResult = col.Get(nameof(this.SpeechResult));
            this.Digits = col.Get(nameof(this.Digits));
            this.To = col.Get(nameof(this.To));

            if (string.IsNullOrEmpty(SpeechResult))
            {
                this.SpeechResult = this.Digits;
            }
        }
    }
}
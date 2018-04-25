namespace FeedbackBot
{
    using System.Collections.Generic;

    public class BotReponse
    {
        public string ResponseText { get; set; }

        public string Intent { get; set; }

        public List<string> Contexts { get; set; }

        public bool endCall { get; set; }

        public BotState BotState { get; set; }
    }
}
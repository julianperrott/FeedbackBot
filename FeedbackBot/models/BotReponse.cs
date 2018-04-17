namespace FeedbackBot
{
    public class BotReponse
    {
        public string ResponseText { get; set; }

        public ResponseAction ResponseAction { get; set; }

        public BotState BotState { get; set; }
    }
}
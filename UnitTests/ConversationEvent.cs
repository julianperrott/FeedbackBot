namespace IntegrationTests
{
    public class ConversationEvent
    {
        public string Text { get; set; }
        public string IntentStartsWith { get; set; }
        public string Context { get; set; }

        public ConversationEvent(string text, string intentStart, string context)
        {
            this.Text = text;
            this.IntentStartsWith = intentStart;
            this.Context = context;
        }
    }
}
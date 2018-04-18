namespace FeedbackBot
{
    using System;

    [Serializable]
    public class BotState
    {
        public BotStatus BotStatus { get; set; }

        public bool ParticipationAgreed { get; set; }

        public string VisitRating { get; set; }

        public string VisitSpecialEvent { get; set; }
    }
}
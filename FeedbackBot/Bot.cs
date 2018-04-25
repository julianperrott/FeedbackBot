namespace FeedbackBot.Bot
{
    using System.Linq;
    using ApiAiSDK;

    public class Bot
    {
        public const string DialogFlowAccessToken = "9394786d9d8040d4864c4ea1ef49c71e";

        public const string WelcomeEvent = "startcall";

        public BotReponse MessageReceived(string sessionId, BotState botState, string text)
        {
            if (botState == null)
            {
                botState = new BotState();
                text = WelcomeEvent;
            }

            var response = CreateDialogFlowClient(sessionId).TextRequest(text);

            var speech = response?.Result?.Fulfillment?.Speech;

            bool hasSpeech = !string.IsNullOrEmpty(speech);
            speech = hasSpeech ? speech : "oh dear something went wrong and the call will now end !";

            if (response.Result?.HasParameters ?? false)
            {
                response.Result.Parameters.ToList().ForEach(p => botState.parameters += p.Key + " = " + p.Value.ToString() + ", ");
            }

            return new BotReponse
            {
                BotState = botState,
                ResponseText = speech,
                endCall = response.Result?.Contexts?.Any(s => s.Name == "endcall") ?? false || hasSpeech,
                Intent = response.Result?.Metadata?.IntentName,
                Contexts = response.Result?.Contexts?.Select(c => c.Name)?.ToList()
            };
        }

        private static ApiAi CreateDialogFlowClient(string sessionId)
        {
            var configuration = new AIConfiguration(DialogFlowAccessToken, SupportedLanguage.English);
            configuration.SessionId = sessionId;
            var apiAi = new ApiAi(configuration);
            return apiAi;
        }
    }
}
namespace FeedbackBotAzureFunctions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Script.Serialization;
    using FeedbackBot;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Twilio.TwiML;
    using Twilio.TwiML.Voice;
    using static Twilio.TwiML.Voice.Gather;

    public static class MsgIn
    {
        [FunctionName("MsgIn")]
        public async static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "MsgIn")]HttpRequestMessage req, TraceWriter log)
        {
            var configuration = new HttpConfiguration();
            req.SetConfiguration(configuration);

            log.Info(await req.Content.ReadAsStringAsync());

            var botState = GetBotStatusFromCookie(req, log);

            // Get msg
            var messageRecieved = new MessageReceived(await req.Content.ReadAsFormDataAsync());

            var botResponse = FeedbackBot.Bot.MessageReceived(botState, messageRecieved.SpeechResult);
            botState = botResponse.BotState;

            var response = new TwiMLResult(CreateVoiceResponse(botState, botResponse), log)
                .ExecuteResult(req);

            AddBotStatusCookie(response, botState, log);

            return response;
        }

        private static VoiceResponse CreateVoiceResponse(BotState botState, BotReponse botResponse)
        {
            var voiceResponse = new VoiceResponse();

            if (!string.IsNullOrEmpty(botResponse.ResponseText))
            {
                if (botState.BotStatus == BotStatus.End)
                {
                    voiceResponse.Append(new Say(botResponse.ResponseText, Say.VoiceEnum.Man, language: Say.LanguageEnum.EnGb));
                    voiceResponse.Hangup();
                }
                else
                {
                    var input = new List<InputEnum> { InputEnum.Speech, InputEnum.Dtmf, InputEnum.Dtmf };
                    var gather = new Gather(input: input, timeout: 30, numDigits: 1, language: "en-GB", speechTimeout: "1");
                    gather.Say(botResponse.ResponseText, Say.VoiceEnum.Man, language: Say.LanguageEnum.EnGb);
                    voiceResponse.Append(gather);
                }
            }

            return voiceResponse;
        }

        private const string BotStateCookieName = "BotState";

        public static void AddBotStatusCookie(HttpResponseMessage response, BotState botState, TraceWriter log)
        {
            var json = new JavaScriptSerializer().Serialize(botState);
            log.Info("Cookie out: " + json);
            response.Headers.AddCookies(new List<CookieHeaderValue> { new CookieHeaderValue(BotStateCookieName, json) });
        }

        public static BotState GetBotStatusFromCookie(HttpRequestMessage request, TraceWriter log)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(BotStateCookieName).FirstOrDefault();
            if (cookie != null && !string.IsNullOrEmpty(cookie[BotStateCookieName].Value))
            {
                log.Info("Cookie in: " + cookie[BotStateCookieName].Value);
                var json = cookie[BotStateCookieName].Value;
                json = json.Substring(0, json.LastIndexOf('}') + 1);
                log.Info("json in: " + json);

                var botState = new JavaScriptSerializer().Deserialize<BotState>(json);
                if (botState != null)
                {
                    return botState;
                }
            }

            log.Info("Cookie not found!");

            return new BotState { BotStatus = BotStatus.Greeting };
        }
    }
}
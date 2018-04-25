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
    using FeedbackBot.Bot;
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
            var botState = LoadBotState(req, log);

            // Get msg
            var messageRecieved = new MessageReceived(await req.Content.ReadAsFormDataAsync());

            log.Info("CallSid :" + messageRecieved.CallSid);

            var botResponse = ProcessMessage(botState, messageRecieved);

            var response = new TwiMLResult(CreateVoiceResponse(botResponse), log)
                .ExecuteResult(req);

            SaveBotState(log, botResponse.BotState, response);

            return response;
        }

        public static BotReponse ProcessMessage(BotState botState, MessageReceived messageRecieved)
        {
            return new Bot().MessageReceived(messageRecieved.CallSid, botState, messageRecieved.SpeechResult);
        }

        private static void SaveBotState(TraceWriter log, BotState botState, HttpResponseMessage response)
        {
            AddBotStatusCookie(response, botState, log);
        }

        private static BotState LoadBotState(HttpRequestMessage req, TraceWriter log)
        {
            return GetBotStatusFromCookie(req, log);
        }

        private static VoiceResponse CreateVoiceResponse(BotReponse botResponse)
        {
            var voiceResponse = new VoiceResponse();

            if (botResponse.endCall)
            {
                voiceResponse.Append(new Say(botResponse.ResponseText, Say.VoiceEnum.Man, language: Say.LanguageEnum.EnGb));
                voiceResponse.Hangup();

                // at this point we could persist our result in a db or put on a queue
            }
            else
            {
                var input = new List<InputEnum> { InputEnum.Speech, InputEnum.Dtmf, InputEnum.Dtmf };
                var gather = new Gather(input: input, timeout: 30, numDigits: 1, language: "en-GB", speechTimeout: "1");
                gather.Say(botResponse.ResponseText, Say.VoiceEnum.Man, language: Say.LanguageEnum.EnGb);
                voiceResponse.Append(gather);
            }

            return voiceResponse;
        }

        private const string BotStateCookieName = "BotState";

        private static void AddBotStatusCookie(HttpResponseMessage response, BotState botState, TraceWriter log)
        {
            var json = new JavaScriptSerializer().Serialize(botState);
            log.Info("Cookie out: " + json);
            response.Headers.AddCookies(new List<CookieHeaderValue> { new CookieHeaderValue(BotStateCookieName, json) });
        }

        private static BotState GetBotStatusFromCookie(HttpRequestMessage request, TraceWriter log)
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

            return null;
        }
    }
}
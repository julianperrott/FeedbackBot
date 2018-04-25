namespace FeedbackBotAzureFunctions
{
    using System.Net;
    using System.Net.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public static class StartCall
    {
        [FunctionName("StartCall")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "StartCall")]HttpRequestMessage req)
        {
            var phoneNumberToCall = new PhoneNumber("+");

            // Find your Account Sid and Auth Token at twilio.com/console
            var twilioAccountSid = "";
            var twilioAuthToken = "";

            // Find your Programable Voice app sid at twilio.com/console
            var twilioProgVoiceApplicationSid = "";

            var phoneNumberCallIsFrom = "+";

            TwilioClient.Init(twilioAccountSid, twilioAuthToken);
            var call = CallResource.Create(phoneNumberToCall, phoneNumberCallIsFrom, applicationSid: twilioProgVoiceApplicationSid);

            System.Diagnostics.Debug.WriteLine("Call Sid is: " + call.Sid);

            var callSid = call.Sid;

            return req.CreateResponse(HttpStatusCode.OK, callSid);
        }
    }
}
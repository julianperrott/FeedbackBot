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
            var phonenumber = new PhoneNumber("+");

            // Find your Account Sid and Auth Token at twilio.com/console
            var accountSid = "";
            var authToken = "";

            var applicationSid = "";

            var from = "+";

            TwilioClient.Init(accountSid, authToken);
            var call = CallResource.Create(phonenumber, from, applicationSid: applicationSid);
            System.Diagnostics.Debug.WriteLine(call.Sid);

            var callSid = call.Sid;

            return req.CreateResponse(HttpStatusCode.OK, callSid);
        }
    }
}
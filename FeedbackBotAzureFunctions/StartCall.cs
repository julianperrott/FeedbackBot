namespace FeedbackBotAzureFunctions
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public static class StartCall
    {
        public const string ServerName = "Server";

        [FunctionName("StartCall")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "StartCall/{name}")]HttpRequestMessage req, string name, TraceWriter log)
        {
            log.Info("Starting call: " + name);

            var phonenumber = new PhoneNumber("+447796758877");

            // Find your Account Sid and Auth Token at twilio.com/console
            var accountSid = "AC686c1c39a728d791e6223950b8b8afc7";
            var authToken = "cb15ef9c56c23ebf41af71c850ed5113";

            var applicationSid = "AP5486a2a98f68585bc1b78e65a55852c8";
            
            var from = "+441234607501";

            TwilioClient.Init(accountSid, authToken);
            var call = CallResource.Create(phonenumber, from, applicationSid: applicationSid);
            System.Diagnostics.Debug.WriteLine(call.Sid);
            var callSid = call.Sid;

            log.Info("Call started: " + call.Sid);

            return req.CreateResponse(HttpStatusCode.OK, callSid);
        }
    }
}
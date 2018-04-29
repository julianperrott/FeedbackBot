using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FeedbackBotAzureFunctions
{
    public static class Voices
    {
        [FunctionName("Polly")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Polly")]HttpRequestMessage req, TraceWriter log)
        {
            var request = req.GetQueryNameValuePairs();
            var text = Get(request, "Text");
            var stream = await Say(text);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            return response;
        }

        public static async Task<Stream> Say(string text)
        {
            var client = new AmazonPollyClient("AKIAJERXAQSZQE3O6OKQ", "WVF0KFKuDWO5GgMdWmdmvyq5KgFyutgCktZFTgcb", RegionEndpoint.EUWest1);
            var speechRequest = new SynthesizeSpeechRequest();
            speechRequest.Text = text;
            speechRequest.OutputFormat = OutputFormat.Mp3;
            speechRequest.SampleRate = "22050";
            speechRequest.VoiceId = VoiceId.Brian;
            var sres = await client.SynthesizeSpeechAsync(speechRequest);
            return sres.AudioStream;
        }

        private static string Get(IEnumerable<KeyValuePair<string, string>> items, string name)
        {
            return items.Where(i => i.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                .Select(i => i.Value)
                .FirstOrDefault();
        }
    }
}
namespace FeedbackBotAzureFunctions
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using global::Twilio.TwiML;
    using Microsoft.Azure.WebJobs.Host;

    public class TwiMLResult
    {
        //public TwiMLResult(string twiml, Encoding encoding)
        //{
        //    this.Data = LoadFromString(twiml, encoding);
        //}

        //public TwiMLResult(XDocument twiml)
        //{
        //    this.Data = twiml;
        //}

        //public TwiMLResult(MessagingResponse response)
        //{
        //    if (response != null)
        //    {
        //        this.Data = LoadFromString(response.ToString(), Encoding.Default);
        //    }
        //}

        //public TwiMLResult(MessagingResponse response, Encoding encoding)
        //{
        //    if (response != null)
        //    {
        //        this.Data = LoadFromString(response.ToString(), encoding);
        //    }
        //}

        private TraceWriter log;

        public TwiMLResult(VoiceResponse response, TraceWriter log)
        {
            this.log = log;

            if (response != null)
            {
                this.Data = LoadFromString(response.ToString(), Encoding.Default);
            }
        }

        //public TwiMLResult(VoiceResponse response, Encoding encoding)
        //{
        //    if (response != null)
        //    {
        //        this.Data = LoadFromString(response.ToString(), encoding);
        //    }
        //}

        public XDocument Data { get; protected set; }

        public HttpResponseMessage ExecuteResult(HttpRequestMessage request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK, "", "application/xml");

            if (this.Data == null)
            {
                this.Data = new XDocument(new XElement("Response"));
            }

            response.Content = new XmlContent(this.Data);

            AddCors(request, response);

            return response;
        }

        public void AddCors(HttpRequestMessage request, HttpResponseMessage response)
        {
            const string Origin = "Origin";
            //const string EnableCors = "X-EnableCors";
            const string AccessControlRequestMethod = "Access-Control-Request-Method";
            //const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
            const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
            const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
            //const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

            if (request.Headers.Contains(Origin))
            {
                log.Info("Header origin: " + request.Headers.GetValues(Origin).First());
                response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
            }
            else
            {
                log.Info("Header origin: none");
            }

            // add header to indicate allowed methods
            if (request.Headers.Contains(AccessControlRequestMethod))
            {
                var accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod);
                response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod.First());
            }
        }

        private static XDocument LoadFromString(string twiml, Encoding encoding)
        {
            var stream = new MemoryStream(encoding.GetBytes(twiml));

            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;

            var reader = XmlReader.Create(stream, settings);
            return XDocument.Load(reader);
        }

        public class XmlContent : HttpContent
        {
            private readonly MemoryStream _Stream = new MemoryStream();

            public XmlContent(XDocument document)
            {
                document.Save(_Stream);
                _Stream.Position = 0;
                Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            }

            protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
            {
                _Stream.CopyTo(stream);

                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _Stream.Length;
                return true;
            }
        }
    }
}
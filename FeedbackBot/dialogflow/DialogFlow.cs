namespace FeedbackBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApiAiSDK;

    public class DialogFlow
    {
        private ApiAi apiAi;

        public string accessToken;

        public DialogFlow(string accessToken)
        {
            this.accessToken = accessToken;
            this.apiAi = new ApiAi(new AIConfiguration(accessToken, SupportedLanguage.English));
        }

        private bool TextToIntent(string text, out Dictionary<string, object> parameters, out string intentName)
        {
            parameters = new Dictionary<string, object>();
            intentName = "none";

            var response = apiAi.TextRequest(text);

            if (response != null && !response.IsError && response.Result != null)
            {
                parameters = response.Result.Parameters;
                intentName = response.Result.Metadata.IntentName;
            }

            return !response.IsError;
        }

        private void Log(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
        }

        public string TextToResponse(string text, string allowedIntent, DialogFlowEntityName expectedEntity)
        {
            var response = TextToResponses(text, allowedIntent, new List<DialogFlowEntityName> { expectedEntity });

            if (response.Count == 1)
            {
                return response[response.Keys.First()];
            }

            return string.Empty;
        }

        private Dictionary<string, string> UnknownResponse = new Dictionary<string, string>();

        public Dictionary<string, string> TextToResponses(string text, string allowedIntent, List<DialogFlowEntityName> allowedEntities)
        {
            return TextToResponses(text, new List<string> { allowedIntent }, allowedEntities);
        }

        public Dictionary<string, string> TextToResponses(string text, List<string> allowedIntents, List<DialogFlowEntityName> allowedEntities)
        {
            Dictionary<string, object> parameters = null;
            string intentName = null;

            var success = this.TextToIntent(text, out parameters, out intentName);

            parameters = parameters == null ? parameters : parameters.Where(s => s.Value != null)
                .Where(s => !string.IsNullOrEmpty(s.Value.ToString()))
                .ToDictionary(s => s.Key, s => s.Value);

            if (!success || parameters == null || parameters.Count == 0)
            {
                Log("Response not recognised. TextToIntent returned false");
                return UnknownResponse;
            }

            if (!allowedIntents.Contains(intentName))
            {
                Log("Response not recognised. Expected intent was: " + string.Join(" or ", allowedIntents) + ", but was: " + intentName);
                return UnknownResponse;
            }

            var allowedEntityNames = allowedEntities.Select(s => s.ToString());

            var filteredParameters = parameters.Where(p => allowedEntityNames.Any(e => e.Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)))
                .Where(p => p.Value != null)
                .Where(p => !string.IsNullOrEmpty(p.Value.ToString()))
                .ToDictionary(p => p.Key, p => p.Value.ToString());

            return filteredParameters;
        }
    }
}
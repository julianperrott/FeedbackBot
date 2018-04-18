namespace FeedbackBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApiAiSDK;

    public class DialogFlow
    {
        private ApiAi apiAi;

        public static string accessToken = "9394786d9d8040d4864c4ea1ef49c71e";

        public enum EntityName
        {
            Unknown,
            NumberEntity,
            SpecialEventEntity,
            YesNoEntity
        }

        public DialogFlow()
        {
            apiAi = new ApiAi(new AIConfiguration(accessToken, SupportedLanguage.English));
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

        public DialogFlowResponse TextToResponse(string text, EntityName expectedEntity)
        {
            var response = TextToResponse(text, new List<EntityName> { expectedEntity });
            if (response.Keys.Count == 1)
            {
                return response[response.Keys.First()];
            }

            return DialogFlowResponse.UnknownResponse;
        }

        private Dictionary<EntityName, DialogFlowResponse> UnknownResponse = new Dictionary<EntityName, DialogFlowResponse> { { EntityName.Unknown, DialogFlowResponse.UnknownResponse } };

        public Dictionary<EntityName, DialogFlowResponse> TextToResponse(string text, List<EntityName> allowedEntities)
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

            var expectedIntentName = "FeedbackIntent";

            if (intentName != expectedIntentName)
            {
                Log("Response not recognised. Expected: " + expectedIntentName + ", but was: " + intentName);
                return UnknownResponse;
            }

            return parameters.Select(ToKeyValue)
                .Where(s => allowedEntities.Contains(s.Key) || s.Key == EntityName.Unknown)
                .ToDictionary(s => s.Key, s => s.Value);
        }

        public KeyValuePair<EntityName, DialogFlowResponse> ToKeyValue(KeyValuePair<string, object> keyValue)
        {
            var result = Enum.GetNames(typeof(DialogFlowResponse))
                .Where(n => n.Equals(keyValue.Value.ToString(), StringComparison.CurrentCultureIgnoreCase))
                .Select(n => (DialogFlowResponse)Enum.Parse(typeof(DialogFlowResponse), n))
                .FirstOrDefault();

            if (result == DialogFlowResponse.NotRecognised)
            {
                Log("Response not recognised. Value is not a known enum: " + keyValue.Key.ToString());
                return UnknownResponse.First();
            }

            var entityName = Enum.GetNames(typeof(EntityName))
                .Where(n => n.Equals(keyValue.Key.ToString(), StringComparison.CurrentCultureIgnoreCase))
                .Select(s => (EntityName)Enum.Parse(typeof(EntityName), s))
                .FirstOrDefault();

            return new KeyValuePair<EntityName, DialogFlowResponse>(entityName, result);
        }
    }
}
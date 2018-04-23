namespace IntegrationTests.BotV1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApiAiSDK;
    using ApiAiSDK.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using BotV1 = FeedbackBot.BotV1;

    [TestClass]
    public class DialogFlowBotV1
    {
        private ApiAi apiAi;

        [TestInitialize]
        public void Initialise()
        {
            var sessionId = Guid.NewGuid().ToString();
            var config = new AIConfiguration(BotV1.Bot.DialogFlowAccessToken, SupportedLanguage.English);
            config.SessionId = sessionId;
            this.apiAi = new ApiAi(config);
        }

        public void AssertParameters(string expectedIntentName, Dictionary<string, string> expected, AIResponse response)
        {
            Assert.IsFalse(response.IsError);

            Assert.AreEqual(expectedIntentName, response.Result.Metadata.IntentName);

            Console.WriteLine("Parameters:");
            response.Result.Parameters.ToList().ForEach(kv => Console.WriteLine($"{kv.Key}, {kv.Value}"));

            expected.ToList().ForEach(kv =>
            {
                Assert.IsTrue(response.Result.Parameters.ContainsKey(kv.Key), $"Failed to find key {kv.Key} in response parameters");
                Assert.AreEqual(kv.Value, response.Result.Parameters[kv.Key], $"Key: {kv.Key}");
            }
            );

            var unknownParameterKeys = response.Result.Parameters.Where(s => !expected.Keys.Contains(s.Key)).ToList();
            if (unknownParameterKeys.Any())
            {
                Console.WriteLine("Unknown parameters:");
                unknownParameterKeys.ForEach(k =>
                {
                    Console.WriteLine($"{k.Key}, {k.Value}");
                });
                Assert.Fail("Unexpected parameters found.");
            }

            Assert.AreEqual(expected.Count, response.Result.Parameters.Count, "Parameter count");
        }

        public void TestParameters(string text, string intent, Dictionary<string, string> expectedParameters)
        {
            var response = apiAi.TextRequest(text);
            AssertParameters(intent, expectedParameters, response);
        }

        [TestMethod]
        public void DialogFlowBotV1_ParticipateIntent()
        {
            var intent = "ParticipateIntent";
            TestParameters("yes", intent, new Dictionary<string, string> { { "YesNoEntity", "Yes" } });
            TestParameters("no", intent, new Dictionary<string, string> { { "YesNoEntity", "No" } });
        }

        [TestMethod]
        public void DialogFlowBotV1_RatingIntent()
        {
            var intent = "RatingIntent";
            TestParameters("it was very poor", intent, new Dictionary<string, string> { { "RatingEntity", "poor" } });
            TestParameters("it was very good", intent, new Dictionary<string, string> { { "RatingEntity", "good" } });
            TestParameters("i found it to be average", intent, new Dictionary<string, string> { { "RatingEntity", "average" } });
            TestParameters("excellent is my rating", intent, new Dictionary<string, string> { { "RatingEntity", "excellent" } });
        }

        [TestMethod]
        public void DialogFlowBotV1_SpecialEventIntent()
        {
            var intent = "SpecialEventIntent";
            TestParameters("I was attending a marriage", intent, new Dictionary<string, string> { { "SpecialEventEntity", "Marriage" } });
            TestParameters("For a birthday party", intent, new Dictionary<string, string> { { "SpecialEventEntity", "Party" } });
        }
    }
}
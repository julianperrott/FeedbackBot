namespace IntegrationTests.BotV1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApiAiSDK;
    using ApiAiSDK.Model;
    using FeedbackBot.Bot;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DialogFlowBot
    {
        public string ContextParticipateYesNo = "participateyesno";
        public string ContextSpecialEvent = "specialevent";
        public string ContextRating = "rating";
        public string ContextEndCall = "endcall";

        private ApiAi apiAi;

        [TestInitialize]
        public void Initialise()
        {
            var sessionId = Guid.NewGuid().ToString();
            var config = new AIConfiguration(Bot.DialogFlowAccessToken, SupportedLanguage.English);
            config.SessionId = sessionId;
            this.apiAi = new ApiAi(config);
        }

        [TestMethod]
        public void DialogFlowBot_Welcome_1_0()
        {
            var response = apiAi.TextRequest(Bot.WelcomeEvent);
            Assert.IsTrue(response.Result.Metadata.IntentName.StartsWith("1.0 "));
            Assert.IsTrue(!string.IsNullOrEmpty(response.Result.Fulfillment.Speech));
            Assert.AreEqual(1, response.Result.Contexts.Count());
            Assert.AreEqual("participateyesno", response.Result.Contexts[0].Name);
        }

        [TestMethod]
        public void DialogFlowBot_ParticipateYes_1_2_ContextPassed()
        {
            var request = new AIRequest
            {
                Contexts = new List<AIContext> { new AIContext { Name = "participateyesno", Lifespan = 5 } },
                Query = new string[1] { "yes" }
            };

            var response = apiAi.TextRequest(request);
            Assert.IsTrue(response.Result.Metadata.IntentName.StartsWith("1.2 "));
            Assert.IsTrue(!string.IsNullOrEmpty(response.Result.Fulfillment.Speech));
            Assert.AreEqual(1, response.Result.Contexts.Count());
            Assert.AreEqual("specialevent", response.Result.Contexts[0].Name);
        }

        [TestMethod]
        public void DialogFlowBot_Path_Happy()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("yes", "1.2 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("yes", "2.0 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("for a party", "3.0 ", ContextRating));
            TestStep(new ConversationEvent("it was good", "4.0 ", ContextEndCall));
        }

        [TestMethod]
        public void DialogFlowBot_Path_NoParticipation()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("no", "1.3 ", ContextEndCall));
        }

        [TestMethod]
        public void DialogFlowBot_Path_EarlySpecialEvent()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("yes", "1.2 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("for a party", "3.0 ", ContextRating));
            TestStep(new ConversationEvent("it was good", "4.0 ", ContextEndCall));
        }

        [TestMethod]
        public void DialogFlowBot_Path_UnMatchedAnswers()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("#", "1.1 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("yes", "1.2 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("#", "3.1 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("yes", "2.0 ", ContextSpecialEvent));

            TestStep(new ConversationEvent("#", "3.1 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("for a party", "3.0 ", ContextRating));
            TestStep(new ConversationEvent("#", "4.1 ", ContextRating));
            TestStep(new ConversationEvent("it was good", "4.0 ", ContextEndCall));
        }

        private void TestStep(ConversationEvent step)
        {
            var response = apiAi.TextRequest(step.Text);
            Assert.IsTrue(response.Result.Metadata.IntentName.StartsWith(step.IntentStartsWith), $"Expected {step.IntentStartsWith} but intent was {response.Result.Metadata.IntentName}");

            var contexts = string.Join(", ", response.Result.Contexts.Select(s => s.Name));
            Assert.AreEqual(step.Context, contexts);
        }
    }
}
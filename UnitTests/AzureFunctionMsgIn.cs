namespace IntegrationTests.BotV1
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using FeedbackBot;
    using FeedbackBot.Bot;
    using FeedbackBotAzureFunctions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AzureFunctionMsgIn
    {
        public string ContextParticipateYesNo = "participateyesno";
        public string ContextSpecialEvent = "specialevent";
        public string ContextRating = "rating";
        public string ContextEndCall = "endcall";

        public string sessionId;

        private BotState botState;

        [TestInitialize]
        public void TestIntialise()
        {
            sessionId = Guid.NewGuid().ToString();
            botState = null;
        }

        [TestMethod]
        public void AzureFunctionMsgIn_Welcome_1_0()
        {
            BotState botState = null;
            var message = new MessageReceived(new NameValueCollection()
            {
                { "CallSid",Guid.NewGuid().ToString() },
                { "SpeechResult", "hi"}
            });

            var response = MsgIn.ProcessMessage(botState, message);

            Assert.IsTrue(response.Intent.StartsWith("1.0 "));
            Assert.IsTrue(!string.IsNullOrEmpty(response.ResponseText));
            Assert.AreEqual(1, response.Contexts.Count());
            Assert.AreEqual("participateyesno", response.Contexts[0]);
        }

        [TestMethod]
        public void AzureFunctionMsgIn_Path_Happy()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("yes", "1.2 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("yes", "2.0 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("for a party", "3.0 ", ContextRating));
            TestStep(new ConversationEvent("it was good", "4.0 ", ContextEndCall));
        }

        [TestMethod]
        public void AzureFunctionMsgIn_Path_NoParticipation()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("no", "1.3 ", ContextEndCall));
        }

        [TestMethod]
        public void AzureFunctionMsgIn_Path_EarlySpecialEvent()
        {
            TestStep(new ConversationEvent(Bot.WelcomeEvent, "1.0 ", ContextParticipateYesNo));
            TestStep(new ConversationEvent("yes", "1.2 ", ContextSpecialEvent));
            TestStep(new ConversationEvent("for a party", "3.0 ", ContextRating));
            TestStep(new ConversationEvent("it was good", "4.0 ", ContextEndCall));
        }

        [TestMethod]
        public void AzureFunctionMsgIn_Path_UnMatchedAnswers()
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
            var message = new MessageReceived(new NameValueCollection()
            {
                { "CallSid",sessionId },
                {"SpeechResult", step.Text}
            });

            var response = MsgIn.ProcessMessage(botState, message);

            Assert.IsTrue(response.Intent.StartsWith(step.IntentStartsWith), $"Expected {step.IntentStartsWith} but intent was {response.Intent}");

            var contexts = string.Join(", ", response.Contexts);
            Assert.AreEqual(step.Context, contexts);

            botState = response.BotState;
        }
    }
}
namespace BotFrameworkTest.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using FeedbackBot;
    using FeedbackBot.Bot;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            BotReponse response = ProcessMessage(context, activity);

            if (!string.IsNullOrEmpty(response.ResponseText))
            {
                await context.PostAsync(response.ResponseText);
            }

            if (response.endCall)
            {
                context.ConversationData.Clear();
                await context.PostAsync("CALL HAS BEEN ENDED");
            }

            context.Wait(MessageReceivedAsync);
        }

        private static BotReponse ProcessMessage(IDialogContext context, Activity activity)
        {
            FeedbackBot.BotState botState = null;

            if (context.ConversationData.ContainsKey("botstate"))
            {
                botState = context.ConversationData.GetValue<FeedbackBot.BotState>("botstate");
            }

            if (!context.ConversationData.ContainsKey("sessionId"))
            {
                context.ConversationData.SetValue("sessionId", Guid.NewGuid().ToString());
            }

            var response = new Bot().MessageReceived(context.ConversationData.GetValue<string>("sessionId"), botState, activity.Text);

            context.ConversationData.SetValue<FeedbackBot.BotState>("botstate", response.BotState);

            return response;
        }
    }
}
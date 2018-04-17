using System;
using System.Threading.Tasks;
using FeedbackBot;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotFrameworkTest.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("CALL HAS BEEN STARTED");
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private FeedbackBot.BotState state = FeedbackBot.BotState.Greeting;

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var response = FeedbackBot.Bot.MessageReceived(state, activity.Text);

            state = response.BotState;

            if (response.ResponseAction == ResponseAction.Say)
            {
                await context.PostAsync(response.ResponseText);
            }
            else if (response.ResponseAction == ResponseAction.EndCall)
            {
                state = FeedbackBot.BotState.Greeting;
                await context.PostAsync(response.ResponseText);
                await context.PostAsync("CALL HAS BEEN ENDED");
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}
namespace BotFrameworkTest.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using FeedbackBot;
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

        private FeedbackBot.BotState state = new FeedbackBot.BotState { BotStatus = BotStatus.Greeting };

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var response = FeedbackBot.Bot.MessageReceived(state, activity.Text);

            state = response.BotState;

            await context.PostAsync($"BotState: {state.BotStatus}, {state.ParticipationAgreed}, {state.VisitRating}, {state.VisitSpecialEvent} ");

            if (!string.IsNullOrEmpty(response.ResponseText))
            {
                await context.PostAsync(response.ResponseText);
            }

            if (state.BotStatus == BotStatus.End)
            {
                state = new FeedbackBot.BotState { BotStatus = BotStatus.Greeting };
                await context.PostAsync("CALL HAS BEEN ENDED");
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}
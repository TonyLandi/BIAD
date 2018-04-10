using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Lab_2_Dialogs_Bot.Dialogs
{
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

            try
            {
                // Try QnaDialog first with a tolerance of 50% match to try to catch a lot of different phrasings
                // The higher the tolerance the more closely the users text must match the questions in QnA Maker
                await context.Forward(new QnaDialog(50), AfterQnA, activity, CancellationToken.None);
            }
            catch (Exception e)
            {
                // If an error occurred with QnA Maker Service, post it out to the user
                await context.PostAsync(e.Message);

                // Wait for the next message from the user
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterJokeOrTrivia(IDialogContext context, IAwaitable<string> result)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterQnA(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity message = null;

            try
            {
                // Our QnaDialog returns an IMessageActivity
                // If the result was something other than an IMessageActivity then some error must have happened
                message = (IMessageActivity)await result;
            }
            catch (Exception e)
            {
                await context.PostAsync($"QnAMaker: {e.Message}");
                // Wait for the next message
                context.Wait(MessageReceivedAsync);
            }

            // If the message summary - NOT_FOUND, then it's time to echo
            if (message.Summary == QnaDialog.NOT_FOUND)
            {
                if (message.Text.ToLowerInvariant().Contains("trivia"))
                {
                    // Since we are not needing to pass any message to start trivia, we can use call instead of forward
                    context.Call(new TriviaDialog(), AfterJokeOrTrivia);
                }
                else
                {
                    // Otherwise, echo...
                    await context.PostAsync($"You said: \"{message.Text}\"");
                    // Wait for the next message
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // Display the answer from QnA Maker Service
                await context.PostAsync(message);
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}
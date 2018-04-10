using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Lab_2_1_Dialogs_Bot_Extra_Exercise.Dialogs
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
                if (activity.Value != null)
                {
                    JToken valueToken = JObject.Parse(activity.Value.ToString());
                    string actionValue = valueToken.SelectToken("action") != null ? valueToken.SelectToken("action").ToString() : string.Empty;

                    if (!string.IsNullOrEmpty(actionValue))
                    {
                        switch (valueToken.SelectToken("action").ToString())
                        {
                            case "jokes":
                                context.Call(new JokesDialog(), AfterJoke);
                                break;
                            case "trivia":
                                context.Call(new TriviaDialog(), AfterTrivia);
                                break;
                            default:
                                await context.PostAsync($"I don't know how to handle the action \"{actionValue}\".");
                                context.Wait(MessageReceivedAsync);
                                break;
                        }
                    }
                    else
                    {
                        await context.PostAsync("It looks like no \"data\" was defined for this.  Check your adaptive cards JSON definition.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    // Try QnaDialog first with a tolerance of 50% match to try to catch a lot of different phrasings
                    // The higher the tolerance the more closely the users text must match the questions in QnA Maker
                    await context.Forward(new QnaDialog(50), AfterQnA, activity, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                // IF an error occured with QnAMaker, post it out to the user
                await context.PostAsync(e.Message);

                // Wait for the next message from the user
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterJoke(IDialogContext context, IAwaitable<string> result)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterTrivia(IDialogContext context, IAwaitable<string> result)
        {
            await context.PostAsync("Thanks for playing!");
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
                    context.Call(new TriviaDialog(), AfterTrivia);
                }
                else if (message.Text.ToLowerInvariant().Contains("jokes"))
                {
                    context.Call(new JokesDialog(), AfterJoke);
                }
                else
                {
                    // Otherwise, echo...
                    await context.PostAsync($"You said: \"{message.Text}\"");
                    // Wait for the nest message
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // Display the answer from QnA Maker
                await context.PostAsync(message);
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}
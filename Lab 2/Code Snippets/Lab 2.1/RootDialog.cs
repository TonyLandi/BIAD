using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace LabBot.Dialogs
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

                    // Try to parse the JSON value to a string where the token equals an "action"
                    JToken valueToken = JObject.Parse(activity.Value.ToString());
                    var actionValue = valueToken.SelectToken("action") != null
                        ? valueToken.SelectToken("action").ToString()
                        : string.Empty;

                    if (!string.IsNullOrEmpty(actionValue))
                    {
                        switch (valueToken.SelectToken("action").ToString())
                        {
                            // If the user clicks "Tell me a joke" reply that you are still working on this feature
                            case "jokes":
                                await context.PostAsync("Sorry, I'm learning new jokes. Come back later.");
                                break;
                            // If the user clicks the "Play Trivia" button, call the trivia dialog to start the game
                            case "trivia":
                                context.Call(new TriviaDialog(), AfterJokeOrTrivia);
                                break;
                            // If it was something else, tell the user that you don't know how to handle the action
                            default:
                                await context.PostAsync($"That does not compute. I am not programmed to \"{actionValue}\".");
                                context.Wait(MessageReceivedAsync);
                                break;
                        }
                    }
                    else
                    {
                        // Tell the user that you are having problems with your JSON
                        await context.PostAsync("It looks like no \"data\" was defined for this action.  Check your adaptive cards JSON definition.");
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
            if (message.Summary == QnaDialog.NotFound)
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

        private async Task AfterJokeOrTrivia(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceivedAsync);
        }
    }
}

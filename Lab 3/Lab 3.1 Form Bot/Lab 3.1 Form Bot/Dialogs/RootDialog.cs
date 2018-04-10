using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Lab_3_1_Form_Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     This is the default handler for any message recieved from the user, we will start by using QnAMaker
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            try
            {
                // if the activity has a Value populated then it is the json from our adaptive card
                if (activity.Value != null)
                {
                    // parse the value field from our activity, this will be populated with the "data" field from our 
                    // adaptive card json if the user clicked the card's button, it will be an empty {{}}
                    // if our json did not define a "data" field
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
                                await context.PostAsync($"I don't know how to handle the action \"{actionValue}\"");
                                context.Wait(MessageReceivedAsync);
                                break;

                        }
                    }
                    else
                    {
                        await context.PostAsync("It looks like no \"data\" was defined for this. Check your adaptive cards json definition.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    // try QnAMaker first with a tolerance of 50% match to try to catch a lot of different phrasings
                    // the higher the tolerance the more closely the users text must match the questions in QnAMaker
                    await context.Forward(new QnaDialog(50), AfterQnA, activity, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                // if an error occured with QnAMaker post it out to the user
                await context.PostAsync(e.Message);
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterJoke(IDialogContext context, IAwaitable<string> result)
        {
            context.Wait(MessageReceivedAsync);
        }

                              /// <summary>
                              /// Ask if the user wants to take a survey after the trivia game.
                              /// </summary>
                              /// <param name="context"></param>
                              /// <param name="result"></param>
                              /// <returns></returns>
        private async Task AfterTrivia(IDialogContext context, IAwaitable<string> result)
        {
            PromptDialog.Confirm(context, AfterAskingAboutSurvey, "Would you like to take a survey?");
        }

        private async Task AfterAskingAboutSurvey(IDialogContext context, IAwaitable<bool> result)
        {
            bool takeSurvey = await result;
            if (!takeSurvey)
            {
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                var survey = new FormDialog<SurveyDialog>(new SurveyDialog(), SurveyDialog.BuildForm, FormOptions.PromptInStart, null);
                context.Call<SurveyDialog>(survey, AfterSurvey);
            }
        }

        private async Task AfterSurvey(IDialogContext context, IAwaitable<SurveyDialog> result)
        {

            SurveyDialog survey = await result;
            await context.PostAsync("Thanks for filling out the form! One last question. Please, describe how the game made you feel.");
            context.Call(new AskLuis(), AfterAskLuis);
        }

        private Task AfterAskLuis(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     This will get called after returning from the QnA
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task AfterQnA(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity message = null;
            try
            {
                // our QnADialog returns an IMessageActivity 
                // if the result was something other than an IMessageActivity then some error must have happened
                message = (IMessageActivity)await result;
            }
            catch (Exception e)
            {
                await context.PostAsync($"QnAMaker: {e.Message}");
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
            // if that messages summary = NO_FOUND then it's time to echo
            if (message.Summary == QnaDialog.NOT_FOUND)
            {
                // if they mentioned trivia start a game!
                if (message.Text.ToLowerInvariant().Contains("trivia"))
                {
                    // since we're not needing to pass any messag to start trivia we can use call instead of forward
                    context.Call(new TriviaDialog(), AfterTrivia);
                }
                else if (message.Text.ToLowerInvariant().Contains("joke"))
                {
                    context.Call(new JokesDialog(), AfterJoke);
                }
                else
                {
                    // echo back to the user, then take over the world!
                    await context.PostAsync($"You said: \"{message.Text}\"");
                    // wait for the next message
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // display the answer from QnAMaker
                await context.PostAsync(message.Text);
            }
        }

    }
}
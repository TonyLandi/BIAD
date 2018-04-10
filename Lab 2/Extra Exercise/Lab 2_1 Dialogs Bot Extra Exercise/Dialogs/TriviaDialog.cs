using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Lab_2_1_Dialogs_Bot_Extra_Exercise.Dialogs
{
    [Serializable]
    public class TriviaDialog : IDialog<string>
    {
        private TriviaGame _game = null;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Welcome to Trivia, Let's play...");
            // post the question and choices as a hero card
            _game = new TriviaGame("");
            await context.PostAsync(_game.CurrentQuestion().Question);
            await context.PostAsync(MakeChoiceCard(context, _game.CurrentQuestion()));
            // wait for input
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        ///     Here we'll check the user's answer and post the next question until there are no more questions
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = (IMessageActivity)await result;
            await context.PostAsync($"You chose: {activity.Text}");
            int usersAnswer = -1;
            if (int.TryParse(activity.Text, out usersAnswer))
            {
                if (_game.Answer(usersAnswer))
                {
                    await context.PostAsync("Correct!");
                }
                else
                {
                    await context.PostAsync("Sorry, that's wrong :-(");
                }
                await context.PostAsync($"Your score is: {_game.Score()}/{_game._questions.Count}. Next question!");
                TriviaQuestion nextQuestion = _game.MoveToNextQuestion();
                if (nextQuestion != null)
                {
                    await context.PostAsync(nextQuestion.Question);
                    await context.PostAsync(MakeChoiceCard(context, nextQuestion));
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    await context.PostAsync("That's it! Thanks for playing :-)");
                    context.Done("");
                }
            }
            else
            {
                await context.PostAsync("I didn't quite get that, I am only programmed to accept numbers :-(");
                context.Wait(MessageReceivedAsync);
            }
        }

        private IMessageActivity MakeChoiceCard(IDialogContext context, TriviaQuestion question)
        {
            var activity = context.MakeMessage();
            // make sure the attachments have been initialized, we use the attachments to add buttons to the activity message
            if (activity.Attachments == null)
            {
                activity.Attachments = new List<Attachment>();
            }

            var actions = new List<CardAction>();
            int choiceIndex = 0;
            foreach (string item in question.Choices)
            {
                actions.Add(new CardAction
                {
                    Title = $"({choiceIndex}) {item}",
                    Value = $"{choiceIndex}",
                    Type = ActionTypes.PostBack // PostBack means the Value will be sent back to the dialog as if the user typed it but it will be hidden from the chat window
                });
                choiceIndex++;
            }
            // create a hero card to "hold" the buttons and add it to the message activities attachments
            activity.Attachments.Add(
                new HeroCard
                {
                    Title = $"Choose One",
                    Buttons = actions
                }.ToAttachment()
            );

            return activity;
        }
    }
}
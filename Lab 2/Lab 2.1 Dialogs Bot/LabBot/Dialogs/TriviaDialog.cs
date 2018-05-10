using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace LabBot.Dialogs
{
    [Serializable]
    public class TriviaDialog : IDialog<string>
    {
        private TriviaGame _game;

        public async Task StartAsync(IDialogContext context)
        {
            // Welcome the user
            await context.PostAsync($"Welcome to Trivia, Let's play...");

            // Post the question and choices as a hero card
            _game = new TriviaGame("");
            await context.PostAsync(_game.CurrentQuestion().Question);
            await context.PostAsync(MakeChoiceCard(context, _game.CurrentQuestion()));

            // Wait for the user to answer
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        ///     Here we'll check the user's answer and post the next question until there are no more questions
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = (IMessageActivity)await result;

            // Reply back to the user with thier answer
            await context.PostAsync($"You chose: {activity.Text}");

            // Let them know if they got it right
            if (int.TryParse(activity.Text, out var usersAnswer))
            {
                if (_game.Answer(usersAnswer))
                {
                    await context.PostAsync("Correct!");
                }
                else
                {
                    await context.PostAsync("Sorry, that's wrong :-(");
                }

                // Show the user thier current score
                await context.PostAsync($"Your score is: {_game.Score()}/{_game.Questions.Count}. Next question!");

                // Move to the next question
                var nextQuestion = _game.MoveToNextQuestion();

                // ...until we run out of questions
                if (nextQuestion != null)
                {
                    await context.PostAsync(nextQuestion.Question);
                    await context.PostAsync(MakeChoiceCard(context, nextQuestion));
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    // Thank the user for playing
                    await context.PostAsync("That's it! Thanks for playing :-)");
                    context.Done("");
                }
            }
            else
            {
                // Handle input validations
                await context.PostAsync("I didn't quite get that, I am only programmed to accept numbers :-(");
                context.Wait(MessageReceivedAsync);
            }
        }

        // Assembles a Hero Card
        private IMessageActivity MakeChoiceCard(IDialogContext context, TriviaQuestion question)
        {
            var activity = context.MakeMessage();

            // make sure the attachments have been initialized, we use the attachments to add buttons to the activity message
            if (activity.Attachments == null)
            {
                activity.Attachments = new List<Attachment>();
            }

            var actions = new List<CardAction>();
            var choiceIndex = 0;

            // For each card action, add a choice from the question to the action buttons
            foreach (var item in question.Choices)
            {
                actions.Add(new CardAction
                {
                    Title = $"({choiceIndex}) {item}",
                    Value = $"{choiceIndex}",

                    // PostBack means the Value will be sent back to the dialog as if the user 
                    // typed it but it will be hidden from the chat window
                    Type = ActionTypes.PostBack
                });

                // Index the choices
                choiceIndex++;
            }

            // Create a hero card to "hold" the buttons and add it to the message activities attachments
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
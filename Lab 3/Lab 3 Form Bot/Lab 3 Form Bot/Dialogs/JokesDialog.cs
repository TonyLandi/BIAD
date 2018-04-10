using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Lab_3_Form_Bot.Dialogs
{
    [Serializable]
    public class JokesDialog : IDialog<string>
    {
        private int _badBotTally = 0;
        private KnockKnockJoke _joke;
        public async Task StartAsync(IDialogContext context)
        {
            _joke = KnockKnockJokes.GetRandomJoke();
            // start by giving the intro, "Knock! Knock!", of course
            await context.PostAsync(_joke.Introduction);
            context.Wait(WaitForWhoseThere);
        }

        public async Task WaitForWhoseThere(IDialogContext context, IAwaitable<object> result)
        {
            string answer = ((IMessageActivity)await result).Text;
            if (answer.ToLower().Contains("who") && answer.ToLower().Contains("there"))
            {
                await context.PostAsync($"{_joke.FirstName}.");
                context.Wait(WaitForSomebodyWho);
            }
            else
            {
                await context.PostAsync("You're supposed to say \"Who's There?\"");
                await BadBot(context);
                context.Wait(WaitForWhoseThere);
            }
        }

        public async Task WaitForSomebodyWho(IDialogContext context, IAwaitable<object> result)
        {
            _badBotTally = 0; // restart bad answer tally
            string answer = ((IMessageActivity)await result).Text;

            if (answer.ToLower().Contains(_joke.FirstName.ToLower()) && answer.ToLower().Contains("who"))
            {
                // say the punchline
                await context.PostAsync(_joke.PunchLine);
                // done!
                context.Done("");
            }
            else
            {
                await context.PostAsync("I don't think you know who knock knock jokes work.");
                await BadBot(context);
                context.Wait(WaitForSomebodyWho);
            }
        }

        public async Task BadBot(IDialogContext context)
        {
            _badBotTally++;
            if (_badBotTally >= 3)
            {
                await context.PostAsync("I'm sorry, but this just isn't working. No more jokes!");
                context.Done("");
            }
        }
    }
}
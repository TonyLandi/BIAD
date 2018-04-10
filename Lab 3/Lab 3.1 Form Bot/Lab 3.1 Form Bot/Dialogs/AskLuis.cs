using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_3_1_Form_Bot.Dialogs
{
    [Serializable]
    public class AskLuis : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity activity = (IMessageActivity)await result;
            // just forward straight to LUIS
            await context.Forward(new LuisAnswers(), AfterLuis, activity, CancellationToken.None);
        }


        public async Task AfterLuis(IDialogContext context, IAwaitable<string> message)
        {
            // we don't have to do any casting becuase we explicitly said we will return a string in MyLuisDialog<string>
            string val = await message;
            if (val == "none") // if no luis intent was found wait for another user input
            {
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done("");
            }
        }
    }
}
        private async Task AfterJokeOrTrivia(IDialogContext context, IAwaitable<object> result)
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

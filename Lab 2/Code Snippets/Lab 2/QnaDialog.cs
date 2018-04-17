using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using QnAMakerDialog;
using QnAMakerDialog.Models;

namespace EchoBot.Dialogs
{
    /// <summary>
    /// Make sure you go to "Manage Nuget Packages" and add QnAMakerDialog by Garry Pretty for this code to work
    /// </summary>
    [Serializable]

    // This is required even though we are reading from the Web.config in th QnaDialog initializer below
    // if you forget this tag you will get a 400 bad request error from QnA Maker Service
    [QnAMakerService("", "")]
    public class QnaDialog : QnAMakerDialog<object>
    {
        // when no match is found in QnA Maker we'll return a message with this in the summary
        public const string NotFound = "NOT_FOUND";
        private readonly float _tolerance;

        public QnaDialog(float tolerance)
        {
            // initialize the tolerance passed in on instantiation
            _tolerance = tolerance;

            // setup the KnowledgeBaseId and SubscriptionKey from the Web.config
            KnowledgeBaseId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];
            SubscriptionKey = ConfigurationManager.AppSettings["QnASubscriptionKey"];
        }

        /// <summary>
        /// The DefaultMatchHandler is called whenver any match is found in QnA Maker Service, no matter how high the score
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="originalQueryText">The text the user sent to the bot</param>
        /// <param name="result">The result returned from the QnA Maker service</param>
        public override Task DefaultMatchHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            // create a new message to return
            var message = context.MakeMessage();

            // init as NOT_FOUND
            message.Summary = NotFound;

            // keep the original user's text in case the calling dialog wants to use it
            message.Text = originalQueryText;

            // find the best score of the matches
            float bestMatch = result.Answers.Max(a => a.Score);

            // if the best matching score is greater than our tolerance, use it
            if (!(bestMatch >= _tolerance)) return Task.CompletedTask;

            // send back the answer from QnA Maker Service as the message text
            // Add code to format QnAMakerResults 'result'
            message.Summary = "";

            // Our results come back as a collection of strings, 
            // for this example we only want the first one
            var answer = result.Answers.First().Answer;

            // Create a Reply activity for the existing dialog stack
            var reply = ((Activity)context.Activity).CreateReply();

            // Parse the pipe delimited response
            const char responseDelimiter = '|';

            var qnaAnswerData = answer.Split(responseDelimiter);
            var title = qnaAnswerData[0];
            var description = qnaAnswerData[1];
            var url = qnaAnswerData[2];
            var imageURL = qnaAnswerData[3];

            // Create a response in the proper format
            if (title == "")
            {
                // Simple response, no UI card
                context.PostAsync(answer.Trim(responseDelimiter));
            }
            else
            {
                // A formatted card with interactive elements
                var card = new HeroCard
                {
                    Title = title,
                    Subtitle = description,
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
                    },
                    Images = new List<CardImage>
                    {
                        new CardImage(url = imageURL)
                    }
                };

                reply.Attachments.Add(card.ToAttachment());
                context.PostAsync(reply);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// When no match at all is found in QnA NoMatchHandler is called
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="originalQueryText">The text the user sent to the bot</param>
        /// <returns></returns>
        public override Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            // create a new message to return
            var message = context.MakeMessage();

            // mark it as NOT_FOUND
            message.Summary = NotFound;

            // keep original text in case the calling dialog needs it
            message.Text = originalQueryText;

            // finish the dialog and return the message to the calling dialog
            context.Done(message); 
            return Task.CompletedTask;
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

namespace LabBob.Dialogs
{
    [Serializable]
    public class QnaDialog : QnAMakerDialog
    {
        public QnaDialog() : base(new
            QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnaSubscriptionKey"],
                ConfigurationManager.AppSettings["QnaKnowledgebaseId"],
                "Sorry, I couldn't find an answer for that", 0.5)))
        {
        }

        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message,
            QnAMakerResults result)
        {
            // Add code to format QnAMakerResults 'result'

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
                await context.PostAsync(answer.Trim(responseDelimiter));
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
                    },
                };

                // Attach the card to the reply activity before posting it back to the user
                reply.Attachments.Add(card.ToAttachment());
                await context.PostAsync(reply);

            }
        }
    }
}
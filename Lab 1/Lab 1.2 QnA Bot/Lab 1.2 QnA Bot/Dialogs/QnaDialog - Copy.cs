using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

namespace QnA_Bot.Dialogs
{
    [Serializable]
    public class QnaDialog : QnAMakerDialog
    {
        public QnaDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnaSubscriptionKey"], ConfigurationManager.AppSettings["QnaKnowledgebaseId"], "Sorry, I couldn't find an answer for that", 0.5)))
        {
        }

        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            // Add code to format QnAMakerResults 'result'

            // Answer is a string
            var answer = result.Answers.First().Answer;
            Activity reply = ((Activity)context.Activity).CreateReply();
            
            string[] qnaAnswerData = answer.Split(';');
            string title = qnaAnswerData[0];
            string description = qnaAnswerData[1];
            string url = qnaAnswerData[2];
            string imageURL = qnaAnswerData[3];

            if (title=="")
            {
                char charsToTrim = ';';
                await context.PostAsync(answer.Trim(charsToTrim));
            }
                            
            else
            {
                HeroCard card = new HeroCard
                {
                    Title = title,
                    Subtitle = description,
                };
                card.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
            };
                card.Images = new List<CardImage>
            {
                new CardImage( url = imageURL)
            };
                reply.Attachments.Add(card.ToAttachment());
                await context.PostAsync(reply);
            }
            
        }
    }
}
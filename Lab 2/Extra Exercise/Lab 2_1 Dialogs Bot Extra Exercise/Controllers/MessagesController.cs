using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AdaptiveCards;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Lab_2_1_Dialogs_Bot_Extra_Exercise
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = message;
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (update.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in update.MembersAdded)
                        {
                            // the bot is always added as a user of the conversation, since we don't
                            // want to display the adaptive card twice ignore the conversation update triggered by the bot
                            if (newMember.Name.ToLower() != "bot")
                            {
                                try
                                {
                                    // read the json in from our file
                                    string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\MyCard.json"));
                                    // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                                    AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
                                    // put the adaptive card as an attachment to the reply message
                                    reply.Attachments.Add(new Attachment
                                    {
                                        ContentType = AdaptiveCard.ContentType,
                                        Content = card
                                    });
                                }
                                catch (Exception e)
                                {
                                    // if an error occured add the error text as the message
                                    reply.Text = e.Message;
                                }
                                await client.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }
    }
}
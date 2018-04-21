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
using EchoBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Lab_2_1_Dialogs_Bot
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
                await Conversation.SendAsync(activity, () => new RootDialog());
                //await Conversation.SendAsync(activity, () => new Dialogs.QnaDialog());
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
            switch (message.Type)
            {
                // Implement when a user joins the conversation
                case ActivityTypes.ContactRelationUpdate:
                    // The bot is always added as a user in the conversation,
                    // Since we don't want the adaptive card to display twice, 
                    // we'll ignore the second conversation that is triggered by the bot.
                    break;

                // Implement when a user leaves the conversation
                case ActivityTypes.DeleteUserData:
                    // If we handle user deletion, return a real message
                    break;

                // The user is typing
                case ActivityTypes.Typing:
                    break;

                // Bounce a message off of the server without replying or changing it's state
                case ActivityTypes.Ping:
                    break;

                case ActivityTypes.ConversationUpdate:

                    // The referenced conversation is being updated
                    IConversationUpdateActivity update = message;

                    // Within the scope of this conversation
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        var client = scope.Resolve<IConnectorClient>();
                        if (update.MembersAdded.Any())
                        {
                            var reply = message.CreateReply();
                            foreach (var newMember in update.MembersAdded)
                            {
                                // the bot is always added as a user of the conversation, since we don't
                                // want to display the adaptive card twice ignore the conversation update 
                                // triggered by the bot
                                if (newMember.Name.ToLower() == "bot") continue;
                            
                                try
                                {
                                    // read the json in from our file
                                    var json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\MyCard.json"));

                                    // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                                    var card = JsonConvert.DeserializeObject<AdaptiveCard>(json);

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

                    break;

            }
        }
    }
}
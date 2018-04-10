using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace $rootnamespace$
{
    [BotAuthentication]
    public class $safeitemname$ : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // TODO: put message-handling logic here

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
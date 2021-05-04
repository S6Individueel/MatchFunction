using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using MatchFunction.Model;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Security.Claims;
namespace MatchFunction
{
    public class Match 
    {
        private static HttpClient httpClient = new HttpClient();


        /// <summary>
        /// Routes the user based on the input given to be added to a group using a userId. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="group"></param>
        /// <param name="userId"></param>
        /// <param name="signalRGroupActions"></param>
        /// <returns></returns>
        [FunctionName("AddToGroup")] //TODO: Possibly replace the userID to join with a connectionID for more individuality
        public static Task AddToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/add/{userId}")] HttpRequest req,
        string group,
        string userId,
        [SignalR(HubName = "matchingHub")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userId,
                    GroupName = group,
                    Action = GroupAction.Add
                });
        }
        /// <summary>
        /// Dynamically sends a message based on the group set by the user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="group"></param>
        /// <param name="signalRMessages"></param>
        /// <returns></returns>
        [FunctionName("SendMessageToGroup")]
        public static Task SendMessageToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/send")] object message,
        string group,
        [SignalR(HubName = "matchingHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Quote rndQuote = new Quote();
            rndQuote.body = message.ToString();
            rndQuote.author = "New Group Message";

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = group,
                    Arguments = new[] { rndQuote }
                });
        }

        [FunctionName("SendQuote")]
        public static async Task SendQuote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "matchingHub")] IAsyncCollector<SignalRMessage> signalRMessage,
            ILogger log)
        {

            var response = await httpClient.GetAsync("https://stoicquotesapi.com/v1/api/quotes/random");
            string responseBody = await response.Content.ReadAsStringAsync();
            Quote incomingQuote = JsonConvert.DeserializeObject<Quote>(responseBody);
            incomingQuote.body = await req.ReadAsStringAsync();

            await signalRMessage.AddAsync(
                new SignalRMessage
                {
                    Target = "incomingQuote", //Should be the same in the client when you define receiving method name
                    Arguments = new[] { incomingQuote }
                });
        }
    }
}


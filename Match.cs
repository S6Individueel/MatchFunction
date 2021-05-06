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

        [FunctionName("Negotiate")]
        public static SignalRConnectionInfo Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequest req,
        [SignalRConnectionInfo(HubName = "chat", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo, //For some reason admin needs to be userID in order to add groups
        ILogger log)
        {
            return connectionInfo;
        }

        [FunctionName("AddToGroup")]
        public static Task AddToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/add/{userId}")] HttpRequest req,
        string group,
        string userId,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userId,
                    GroupName = group,
                    Action = GroupAction.Add
                });
        }
        [FunctionName("AddToGroupGrimly")]
        public static Task AddToGroupGrimly(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        ClaimsPrincipal claimsPrincipal,
        [SignalR(HubName = "chat")]
        IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userIdClaim.Value,
                    GroupName = "groupName",
                    Action = GroupAction.Add
                });
        }
        /*        [FunctionName("addToGroupGrimly")] //Altered from Grimly src=https://github.com/MicrosoftDocs/azure-docs/issues/34409
                public static async Task<Task> AddToGroupGrimly(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
                ClaimsPrincipal claimsPrincipal,
                [SignalR(HubName = "chat")]
                        IAsyncCollector<SignalRGroupAction> signalRGroupActions)
                {
                    var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
                    //string groupName = await req.ReadAsStringAsync();
                    return signalRGroupActions.AddAsync(
                        new SignalRGroupAction
                        {
                            UserId = userIdClaim.Value,
                            GroupName = "myGroup",
                            Action = GroupAction.Add
                        });
                }*/

        [FunctionName("SendMessageToGroup")]
        public static Task SendMessageToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/send")] object message,
        string group,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
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

        [FunctionName("SendAnswersToGroup")]
        public static Task SendAnswersToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/send/answers")] object message,
        string group,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            MatchList matchList = new MatchList
            {
                User = "User",
                MatchResults = message.ToString()
            };

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "incomingList",
                    Arguments = new[] { matchList }
                });
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
        }
        [FunctionName("SendQuote")]
        public static async Task SendQuote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessage,
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


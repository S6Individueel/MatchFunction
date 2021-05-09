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
using System.Collections.Generic;

namespace MatchFunction
{
    public class Match 
    {

        private static HttpClient httpClient = new HttpClient();
        private static Random rnd = new Random();
        private static readonly string[] sentences = new string[] { "momentarily", "bloodthirstily", "unnecessarily", "trustworthily", "involuntarily", "secondarily", "mandatorily", "temporarily", "arbitrarily", "voluntarily", "ordinarily", "stealthily", "unsanitarily", "worthily", "unworthily", "sanitarily", "squekily", "hungrily", "cheekily" };
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

        [FunctionName("HostGroup")]
        public static async Task<Task> HostGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "host/{userId}")] HttpRequest req,
        string userId,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            string groupCode = await IdGenerator.GetBase36(6);
            await signalRGroupActions.AddAsync( //Simply adds the user to the group, but doesn't return the request yet
                new SignalRGroupAction
                {
                    UserId = userId,
                    GroupName = groupCode,
                    Action = GroupAction.Add
                }); 

            return signalRMessages.AddAsync( //Return the message with the code and user in order for quick initialization client side, also reduces roundtrip by 1
            new SignalRMessage
            {
                GroupName = groupCode,
                Target = "incomingHost",
                Arguments = new[] { groupCode }
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/{userid}/send")] object message,
        string group,
        string userid,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.Content = message.ToString();
            chatMessage.UserName = userid;

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = group,
                    Arguments = new[] { chatMessage }
                });
        }

        [FunctionName("SendUpdateToGroup")]
        public static Task SendUpdateToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/updategroup")] object message, //contains chatmessage with latest user, userslist in room and the funny
        string group,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            ChatMessage updateMessage = JsonConvert.DeserializeObject<ChatMessage>(message.ToString());

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "incomingUser",
                    Arguments = new[] { updateMessage }
                });
        }

        [FunctionName("SendSwipeDataToGroup")]
        public static Task SendSwipeDataToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/start/{selectedData}")] object message, //contains chatmessage with latest user, userslist in room and the funny
        string group,
        string selectedData,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
                return signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        GroupName = group,
                        Target = "incomingData",
                        Arguments = new[] { selectedData }
                    });

        }

        [FunctionName("SendUpdateToHost")]
        public static Task SendUpdateToHost(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{group}/updatehost")] object message, //contains username to update
        string group,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {

            ChatMessage updateMessage = new ChatMessage
            {
                UserName = message.ToString(),
                Content = "has joined the room " + sentences[rnd.Next(sentences.Length)]
            };

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "incomingUserUpdate",
                    Arguments = new[] { updateMessage }
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

    }
}


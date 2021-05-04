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
    public static class Match
    {
        private static HttpClient httpClient = new HttpClient();


        [FunctionName("SendQuote")]
        public static async Task SendQuote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "MatchingHub")] IAsyncCollector<SignalRMessage> signalRMessage,
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

        [FunctionName("Testing")]
        public static async Task Testing(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "MatchingHub")] IAsyncCollector<SignalRMessage> signalRMessage,
        SignalRConnectionInfo connectionInfo, ILogger log)
        {
            string hub = "testing";
            string group = "newGroup";
            string connectionId = connectionInfo.Url;
            var response = await httpClient.GetAsync($"matchflixchat.service.signalr.net/api/v1/hubs/{hub}/groups/{group}/connections/{connectionId}");

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

        /*        [FunctionName("test")]
                public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req, ILogger log)
                {
                    var payload = new PayloadMessage()
                    {
                        Target = methodName,
                        Arguments = args
                    };
                    var url = $"{endpoint}/api/v1/hubs/{hubName}";
                    var bearer = GenerateJwtBearer(null, url, null, DateTime.UtcNow.AddMinutes(30), accessKey);
                    await PostJsonAsync(url, payload, bearer);

                    await signalR.SendAsync("signinsamplehub", "updateSignInStats", stats.TotalNumber, stats.ByOS, stats.ByBrowser);
                }
                public async Task SendAsync(string hubName, string methodName, params object[] args)
                {
                    var payload = new PayloadMessage()
                    {
                        Target = methodName,
                        Arguments = args
                    };
                    var url = $"{endpoint}/api/v1/hubs/{hubName}";
                    var bearer = GenerateJwtBearer(null, url, null, DateTime.UtcNow.AddMinutes(30), accessKey);
                    await PostJsonAsync(url, payload, bearer);

                    //---
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url)
                    };
                    var content = JsonConvert.SerializeObject(payload);
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    return httpClient.SendAsync(request);
                }*/
        /*        private Task<HttpResponseMessage> PostJsonAsync(string url, object body, string bearer)
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url)
                    };

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.AcceptCharset.Clear();
                    request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));

                    var content = JsonConvert.SerializeObject(body);
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    return httpClient.SendAsync(request);
                }*/

        [FunctionName("addToGroup")]
        public static async Task<Task> AddToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        ClaimsPrincipal claimsPrincipal,
        [SignalR(HubName = "MatchingHub")]
        IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            string friendCode = "";
            if (req.Body != null)
            {
                friendCode = await req.ReadAsStringAsync();
            }

            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userIdClaim.Value,
                    GroupName = friendCode ?? "myGroup",
                    Action = GroupAction.Add
                });
        }

        [FunctionName("SendMessage")]
        public static Task SendMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
        [SignalR(HubName = "MatchingHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
            // the message will be sent to the group with this name
            GroupName = "myGroup",
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
        }
    }
}

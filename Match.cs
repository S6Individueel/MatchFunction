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

namespace MatchFunction
{
    public static class Match
    {
        private static HttpClient httpClient = new HttpClient();

/*        [FunctionName("Match")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "quotes")] IAsyncCollector<Quote>signalRMessage,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var response = await httpClient.GetAsync("https://stoicquotesapi.com/v1/api/quotes/random");
            string responseBody = await response.Content.ReadAsStringAsync();
            var incomingQuote = JsonConvert.DeserializeObject<Quote>(responseBody);

            await signalRMessage.AddAsync(incomingQuote);

            return new OkObjectResult($"{incomingQuote.author}: {incomingQuote.body}");
            //return new OkObjectResult("OK, great");
        }*/

        [FunctionName("SendQuote")]
        public static async Task SendQuote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "quotes")] IAsyncCollector<SignalRMessage> signalRMessage,
            ILogger log
            )
        {
            var response = await httpClient.GetAsync("https://stoicquotesapi.com/v1/api/quotes/random");
            string responseBody = await response.Content.ReadAsStringAsync();
            Quote incomingQuote = JsonConvert.DeserializeObject<Quote>(responseBody);

            await signalRMessage.AddAsync(
                new SignalRMessage
                    {
                        Target = "incomingQuote", //Should be the same in the client when you define receiving method name
                        Arguments = new[] { incomingQuote }
                    });
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "quotes")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}

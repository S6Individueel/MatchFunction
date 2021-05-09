/*using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace MatchFunction
{
    public static class NegotiateHandshake
    {

        [FunctionName("Negotiate")]
        public static SignalRConnectionInfo Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequest req,
        [SignalRConnectionInfo(HubName = "chat", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo, 
        ILogger log)//Headers will be passed from the client as userID which allows everyone to add themselves to a group
        {
            return connectionInfo;
        }


    }
}
*/
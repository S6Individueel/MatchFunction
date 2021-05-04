using System;
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
        ///<summary>
        /// Uses the following link to gain more access to user info.
        ///https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-signalr-service-input?tabs=csharp
        /// </summary>

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "MatchingHub", UserId = "{headers.x-ms-client-principal-id}")] 
            SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        
    }
}

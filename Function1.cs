using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MatchFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("QueueName", Connection = "ConnectionStrings:AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}

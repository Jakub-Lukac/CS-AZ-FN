using System;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace CS_AZ_FN_Functions
{
    public class UsersTimeTrigger
    {
        [Function("UsersTimeTrigger")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}

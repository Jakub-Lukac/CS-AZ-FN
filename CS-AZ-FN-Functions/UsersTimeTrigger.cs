using System;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using CS_AZ_FN_Functions.Helpers;
using System.Collections.Generic;
using Azure.Messaging.EventHubs.Producer;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.WebJobs;
using System.Text;
using Newtonsoft.Json;

namespace CS_AZ_FN_Functions
{
    public class UsersTimeTrigger
    {
        private readonly ILogger<UsersTimeTrigger> _logger;
        private readonly Code _code;

        public UsersTimeTrigger(Code code, ILogger<UsersTimeTrigger> logger)
        {
            _code = code;
            _logger = logger;
        }

        [Timeout("00:10:00")]
        [Function("UsersTimeTrigger")]
        public async Task Run([TimerTrigger("0 0 9-17 * * *")]TimerInfo myTimer)
        // 0 0 9-17 * * * - run every hour, every day between 9-5
        {
            DateTime messageDate = DateTime.Now;
            List<string> logs = new List<string>();
            _code.LogHelper($"C# Timer trigger function executed at: {messageDate}", logs, _logger);

            var parameters = new Model();

            var groups = parameters.groups;

            var graphClient = _code.GetAuthenticatedGraphClient(parameters.tenantId, parameters.appId, parameters.appSecret);

            // This is the client used to send messages (events) to Azure Event Hub.
            // Connects to specified event hub  
            await using var producer = new EventHubProducerClient(parameters.eventHubConnectionString);

            // This is a batch of events to be sent to Azure Event Hub. It ensures that the batch does not exceed the
            // maximum size allowed by Event Hub.
            EventDataBatch eventBatch = await producer.CreateBatchAsync();

            int batchCount = 0;
            int usersCount = 0;
            int batchTotal = 0;
            int usersTotal = 0;

            if(groups.Length == 0)
            {
                _code.LogHelper($"No groups to sync.", logs, _logger);
            }
            else
            {
                _code.LogHelper("Groups to sync : ", logs, _logger);
                foreach (var group in groups)
                {
                    _code.LogHelper($"Group ID : {group}", logs, _logger);

                    // Resets batchCount and usersCount for each group.
                    batchCount = 0;
                    usersCount = 0;

                    var users = await _code.GetUsers(graphClient, group);

                    foreach (var user in users)
                    {
                        var p = new Model()
                        {
                            userId = user.Id,
                            tenantId = parameters.tenantId,
                        };

                        _code.LogHelper($"User Display Name : {user.DisplayName}", logs, _logger);
                        usersCount++;

                        // Creates an EventData object ed containing the serialized p as JSON.
                        Azure.Messaging.EventHubs.EventData ed = new Azure.Messaging.EventHubs.EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(p)));

                        // Attempts to add the EventData object ed to the eventBatch.
                        try
                        {
                            if (!eventBatch.TryAdd(ed))
                            {
                                throw new Exception($"Batch is full.");
                            }
                        }
                        catch
                        {
                            _code.LogHelper($"Batch is full at {eventBatch.Count} items.", logs, _logger);
                        }

                        // Send Batch When Full or Last User
                        if (eventBatch.Count >= 50 || users.IndexOf(user) == users.Count - 1)
                        {
                            // sending the batch
                            await producer.SendAsync(eventBatch);
                            _code.LogHelper($"Batch is sent with {eventBatch.Count} items", logs, _logger);

                            await Task.Delay(4900);
                            // Creates a new EventDataBatch for the next batch of events
                            eventBatch = await producer.CreateBatchAsync();
                            batchCount++;
                        }
                    }

                    _code.LogHelper($"Cycle finished with {batchCount} batches, total of {usersCount} users", logs, _logger);
                    batchTotal += batchCount;
                    usersTotal += usersCount;
                }

                _code.LogHelper($"Trigger finished with {batchTotal} batches, total of {usersTotal} users", logs, _logger);
                await producer.DisposeAsync();
            }

            
        }
    }
}

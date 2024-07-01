using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using CS_AZ_FN_Functions.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CS_AZ_FN_Functions
{
    public class EventHubReader
    {
        private readonly ILogger<EventHubReader> _logger;
        private readonly Code _code;

        public EventHubReader(ILogger<EventHubReader> logger, Code code)
        {
            _logger = logger;
            _code = code;   
        }

        [Timeout("00:30:00")]
        [Function(nameof(EventHubReader))]
        public async Task Run([EventHubTrigger("CONF_EVENTHUB_NAME", Connection = "CONF_EVENTHUB_CONNECTION_STRING", IsBatched = false)] string eventHubAsString)
        {
            // "CONF_EVENTHUB_NAME": The name of the Event Hub from which to read events.
            // This should match the Event Hub name in your Azure Event Hub namespace.

            // Connection = "CONF_EVENTHUB_CONNECTION_STRING": This points to the connection string
            // that allows the function to connect to the Event Hub. The connection string includes
            // the namespace, access key name, and access key.

            // IsBatched = false: This specifies that the function should process events one at a time. If set to true,
            // events would be received in batches.

            List<string> logs = new List<string>();

            try
            {
                try
                {
                    // DeserializeObject<Model> = generic type
                    var parameters = JsonConvert.DeserializeObject<Model>(eventHubAsString);

                    if (parameters == null || parameters.userId == null || parameters.userId == string.Empty)
                    {
                        throw new Exception("No Client ID was specified.");
                    }
                    else
                    {
                        var graphClient = _code.GetAuthenticatedGraphClient(parameters.tenantId, parameters.appId, parameters.appSecret);

                        var user = await _code.GetUser(graphClient, parameters.userId);

                        _code.LogHelper($"User {user.DisplayName} has been received to the Event Hub.", logs, _logger);
                    }
                }
                catch (Exception ex)
                {
                    _code.LogHelper($"{ex.Message}", logs, _logger); 
                }
            }
            catch (Exception ex)
            {
                _code.LogHelper($"Event error : {ex.Message}", logs, _logger);
            }
        }
    }
}

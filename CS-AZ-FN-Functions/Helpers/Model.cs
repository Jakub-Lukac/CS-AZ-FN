using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_AZ_FN_Functions.Helpers
{
    public class Model
    {
        public string tenantId { get; set; }
        [JsonIgnore]
        public string appId { get; set; }
        [JsonIgnore]
        public string appSecret { get; set; }
        [JsonIgnore]
        public string userId { get; set; }
        public string[] groups { get; set; }

        [JsonIgnore]
        public string eventHubConnectionString { get; set; }
        [JsonIgnore]
        public string eventHubName { get; set; }

        public Model()
        {
            tenantId = Environment.GetEnvironmentVariable("CONF_TENANT_ID");
            appId = Environment.GetEnvironmentVariable("CONF_APP_ID");
            appSecret = Environment.GetEnvironmentVariable("CONF_APP_SECRET");

            eventHubConnectionString = Environment.GetEnvironmentVariable("CONF_EVENTHUB_CONNECTION_STRING");
            eventHubName = Environment.GetEnvironmentVariable("CONF_EVENTHUB_NAME");

            groups = Environment.GetEnvironmentVariable("GROUPS").Split(",");
        }
    }
}

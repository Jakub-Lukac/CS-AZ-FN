using CS_AZ_FN_Functions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CS_AZ_FN_Functions
{
    public class UserHttpTrigger
    {
        private readonly ILogger<UserHttpTrigger> _logger;
        private readonly Code _code;

        public UserHttpTrigger(ILogger<UserHttpTrigger> logger, Code code)
        {
            _logger = logger;
            _code = code;
        }

        [Function("UserHttpTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var parameters = new Model();

            // tenantID = 30b63a6d-92e7-4db7-bc45-0d5dd3b77339
            // clientID = beca713b-75d5-471e-8d6e-8c235d7e8ea5
            // clientSecret = 57y8Q~6SLU-36iuaMi-f9rs6PZt3M21kxYyfpbdN

            var graphClient = _code.GetAuthenticatedGraphClient(parameters.tenantId, 
                parameters.appId, parameters.appSecret);

            var result = await _code.GetUser(graphClient, "829f5aaf-1820-4875-bfd3-ad3fc129f95e");

            return new OkObjectResult(result.DisplayName);
        }
    }
}

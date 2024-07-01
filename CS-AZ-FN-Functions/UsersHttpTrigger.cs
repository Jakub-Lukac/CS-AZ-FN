using CS_AZ_FN_Functions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CS_AZ_FN_Functions
{
    public class UsersHttpTrigger
    {
        private readonly ILogger<UsersHttpTrigger> _logger;
        private readonly Code _code;

        public UsersHttpTrigger(ILogger<UsersHttpTrigger> logger, Code code)
        {
            _logger = logger;
            _code = code;
        }

        [Function("UserHttpTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var parameters = new Model();

            var graphClient = _code.GetAuthenticatedGraphClient("TENANT ID", 
                "CLIENT ID", "APP SECRET");

            // Testing
            var result = await _code.GetUser(graphClient, "{User ID}");

            return new OkObjectResult(result.DisplayName);
        }
    }
}

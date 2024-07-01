# CS-AZ-FN

## Project Overview

Demo to showcase how to connect Azure `Time Trigger Function` with `Event Hub Trigger` Function. To authenticate the function we use `Environment variables` from Azure Portal, which are also stored in a key vault. The Time Trigger function was designed to handle any time of traffic (small, big data) by implementing event hub batches.

# Azure Function Setup

## Creating Azure Function

In Visual Studio create Azure Function with HTTP Trigger for starters. Select the .NET 6.0 template. Right after creation upgrade the azure function to .NET 8 version. Right-click on your Azure function and hit Upgrade. Follow the instructions in this official article [dotnet-isolated](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows)</br>

After the upgrade run the Program.cs is automatically created. If you want to include Logging, App Insights, and Dependency Injections in your project use the following code snippet for your Program.cs file

```text
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using AZ_Fn_Graph.Helpers;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<Code>();
    })
    .ConfigureLogging(logging =>
    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
```

## NuGet Packages

Download the following packages

```text
Azure.Identity
Azure.Messaging.EventHubs
Microsoft.ApplicationInsights.WorkerService
Microsoft.Azure.Functions.Extensions
Microsoft.Azure.Functions.Worker
Microsoft.Azure.Functions.Worker.Sdk
Microsoft.Azure.Functions.Worker.ApplicationInsights
Microsoft.Azure.Functions.Worker.Extensions.Http
Microsoft.Azure.Functions.Worker.Extensions.EventHubs
Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore
Microsoft.Graph
```

## Time Trigger Function

To specify when your Azure Function should run using a `CRON expression`, you can use the `TimerTrigger attribute` in Azure Functions. You can read more on [CRON](https://en.wikipedia.org/wiki/Cron). Function goes through specified groups and lists all the users within these groups. Then it creates EventData object containing serialized User object as JSON and if possible adds it to the batch.

## Event Hub Function

This Azure Function triggers upon receiving events from an Event Hub. It `processes each event individually` and retrieves the user's Display Name using the `Graph API` based on the extracted UserID.

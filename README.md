# CS-AZ-FN

## Project Overview

Demo to showcase how to connect Azure `Time Trigger Function` with `Event Hub Trigger` Function. To authenticate the function we use `Environment variables` from Azure Portal, which are also stored in a key vault. The Time Trigger function was designed to handle any time of traffic (small, big data) by implementing event hub batches.

## Time Trigger Function

To specify when your Azure Function should run using a `CRON expression`, you can use the `TimerTrigger attribute` in Azure Functions. You can read more on [CRON](https://en.wikipedia.org/wiki/Cron)
Function goes through specified groups and lists all the users within these groups. Then it creates EventData object containing serialized User object as JSON and if possible adds it to the batch.

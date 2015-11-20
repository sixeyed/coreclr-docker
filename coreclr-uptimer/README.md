FROM sixeyed/coreclr-base

Sample .NET Core app - pings a URL and records the response in Azure storage.

To use, set an environment variable with the connection string for your Azure storage account, and pass the URL to ping:

docker run -e STORAGE_CONNECTION_STRING='DefaultEndpointsProtocol=https;AccountName=***;AccountKey=***' sixeyed/coreclr-uptimer https://blog.sixeyed.com

The container makes a single call, write the response and exits. Use with CRON to schedule regular pings.

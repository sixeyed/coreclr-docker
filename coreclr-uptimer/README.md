FROM sixeyed/coreclr-base

Sample .NET Core app - pings a URL and records the response in Azure storage.

To use, set an environment variable with the connection string for your Azure storage account, and pass the URL to ping and the frequency:

    docker run -e STORAGE_CONNECTION_STRING='DefaultEndpointsProtocol=https;AccountName=|name|;AccountKey=|key|' sixeyed/coreclr-uptimer https://blog.sixeyed.com 00:00:20

At the requested frequency, the app makes a single HTTP GET, and writes the response status and duration to Azure. 

Frequency is parsed as a .NET TimeSpan - format HH:MM:SS.


FROM sixeyed/coreclr-base

Sample .NET Core app - pings a URL and records the response in Azure storage.

To use, set an environment variable with the connection string for your Azure storage account, and pass the URL to ping:

    docker run -e STORAGE_CONNECTION_STRING='DefaultEndpointsProtocol=https;AccountName=|name|;AccountKey=|key|' sixeyed/coreclr-uptimer https://blog.sixeyed.com

The container makes a single HTTP GET, writes the response status and duration to Azure and exits. 

Use with CRON to schedule regular pings, and save your connection string in an environment file if you want to keep the details out of crontab:

    $ crontab -l
    * * * * * docker run --rm --env-file /etc/azure-env.list sixeyed/coreclr-uptimer https://blog.sixeyed.com

    $ cat /etc/azure-env.list
    STORAGE_CONNECTION_STRING='DefaultEndpointsProtocol=https;AccountName=|name|;AccountKey=|key|'



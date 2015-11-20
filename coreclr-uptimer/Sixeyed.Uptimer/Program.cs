using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sixeyed.Uptimer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
            var url = args[0];

            var stopwatch = Stopwatch.StartNew();
            var response = GetResponse(url).Result;
            var result = SaveResponse(storageConnectionString, response, stopwatch.ElapsedMilliseconds).Result;

            Console.WriteLine("Done. Took: {0}ms, success: {1}", stopwatch.ElapsedMilliseconds, result);
        }

        private static async Task<HttpResponseMessage> GetResponse(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            using (var client = new HttpClient())
            {
                return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            }
        }

        private static async Task<bool> SaveResponse(string storageConnectionString, HttpResponseMessage response, long duration)
        {
            var content = string.Format("{0}\t{1}\t{2}\t{3}\n", response.Headers.Date, (int)response.StatusCode, response.ReasonPhrase, duration);
            var md5 = Md5Hash(content);

            try
            {
                var account = CloudStorageAccount.Parse(storageConnectionString);
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("responses");
                await container.CreateIfNotExistsAsync();

                var blobName = string.Format("{0}.txt", DateTime.UtcNow.ToString("yyyyMMddHH"));
                var blobPath = string.Format("{0}/{1}", response.RequestMessage.RequestUri.Host, blobName);
                var blockBlob = container.GetBlockBlobReference(blobPath);

                if (!await blockBlob.ExistsAsync())
                {
                    await blockBlob.UploadFromByteArrayAsync(new byte[] { }, 0, 0);
                }

                await AppendToBlob(blockBlob, content, md5);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string Md5Hash(string content)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var hashFunction = MD5.Create();
            return Convert.ToBase64String(hashFunction.ComputeHash(contentBytes));
        }

        private static async Task AppendToBlob(CloudBlockBlob blockBlob, string content, string md5)
        {
            var leaseId = await blockBlob.AcquireLeaseAsync(TimeSpan.FromSeconds(30), null);
            var access = AccessCondition.GenerateLeaseCondition(leaseId);
            var options = new BlobRequestOptions();
            var context = new OperationContext();

            try
            {
                var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    stream.Position = 0;
                    await blockBlob.PutBlockAsync(blockId, stream, md5, access, options, context);
                }

                var blockList = await blockBlob.DownloadBlockListAsync(BlockListingFilter.Committed, access, options, context);
                var blockIds = blockList.Select(x => x.Name).ToList();
                blockIds.Add(blockId);
                await blockBlob.PutBlockListAsync(blockIds, access, options, context);
            }
            finally
            {
                await blockBlob.ReleaseLeaseAsync(access);
            }
        }
    }
}
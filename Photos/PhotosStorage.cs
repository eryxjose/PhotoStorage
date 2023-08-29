using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Photos.Models;
//using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace Photos
{
    public static class PhotosStorage
    {
        [FunctionName("PhotosStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("photos", FileAccess.ReadWrite, Connection = Literals.StorageConnectionString)] Azure.Storage.Blobs.BlobContainerClient blobContainer,
            [CosmosDB("photos", "metadata", Connection = Literals.CosmosDBConnection, CreateIfNotExists = true, PartitionKey = "/id")] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<PhotoUploadModel>(body);

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            await blobContainer.CreateIfNotExistsAsync();

            //var cloudBlockBlob = blobContainer.GetBlockBlobReference(blobName);
            var cloudBlockBlob = blobContainer.GetBlobClient(blobName);

            var photoBytes = Convert.FromBase64String(request.Photo);
            var stream = new MemoryStream(photoBytes);

            //await cloudBlockBlob.UploadFromByteArrayAsync(photoBytes, 0, photoBytes.Length);
            await cloudBlockBlob.UploadAsync(stream);

            var item = new
            {
                id = newId,
                name = request.Name,
                description = request.Description,
                tags = request.Tags
            };

            await items.AddAsync(item);

            logger?.LogInformation($"Sucesso ao enviar o arquivo {newId}.jpg e gravar metadados.");

            return new OkObjectResult(newId);

        }
    }
}

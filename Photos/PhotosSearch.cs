using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
//using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using Photos.Models;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.ComponentModel;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace Photos
{
    public static class PhotosSearch
    {
        [FunctionName("PhotosSearch")]

        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB("photos", "metadata", Connection = Literals.CosmosDBConnection)] CosmosClient client,
            ILogger logger)
        {
            logger?.LogInformation("Pesquisando...");

            var searchTerm = req.Query["searchTerm"];
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new NotFoundResult();
            }

            var container = client.GetDatabase("photos").GetContainer("metadata");

            using FeedIterator<PhotoUploadModel> setIterator = container.GetItemLinqQueryable<PhotoUploadModel>()
                .Where(b => b.Description.Contains(searchTerm))
                .ToFeedIterator();

            //using FeedIterator<PhotoUploadModel> setIterator = container.GetItemLinqQueryable<PhotoUploadModel>()
            //    .Where(b => b.Description.Contains(searchTerm))
            //    .ToFeedIterator<PhotoUploadModel>();

            //Asynchronous query execution
            var results = new List<dynamic>();

            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    results.Add(item);
                }
            }

            return new OkObjectResult(results);
        }
    }
}

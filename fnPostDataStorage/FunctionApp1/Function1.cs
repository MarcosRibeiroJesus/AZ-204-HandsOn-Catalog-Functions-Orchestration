using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FunctionApp1
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("dataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing a storage upload blob request.");

            try
            {
                if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("Header 'file-type' not found.");
                }

                var fileType = fileTypeHeader.ToString();
                var form = await req.ReadFormAsync();
                var file = form.Files["file"];

                if (file == null)
                {
                    return new BadRequestObjectResult("File not found.");
                }

                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = fileType;

                var blobClient = new BlobClient(connectionString, containerName, file.FileName);
                var containerClient = new BlobContainerClient(connectionString, containerName);

                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

                string blobName = file.FileName;
                var blob = containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                _logger.LogInformation($"Blob {file.FileName} uploaded to container {containerName}.");

                return new OkObjectResult(new
                {
                    Message = $"Blob {file.FileName} uploaded to container {containerName}.",
                    BlobUri = blob.Uri
                });

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }


        }
    }

}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnPostDataBase
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("movie")]
        [CosmosDBOutput(
            databaseName: "%DataBaseName%",
            containerName: "%ContainerName%",
            Connection = "CosmosDBConnection",
            CreateIfNotExists = true,
            PartitionKey = "/id")]
        public async Task<object> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing movie upload request...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                MovieRequest movie = JsonConvert.DeserializeObject<MovieRequest>(requestBody);

                if (movie == null || string.IsNullOrEmpty(movie.title) || string.IsNullOrEmpty(movie.video))
                {
                    return new BadRequestObjectResult("Invalid request: title and video are required.");
                }

                if (string.IsNullOrEmpty(movie.id))
                {
                    movie.id = Guid.NewGuid().ToString();
                }

                _logger.LogInformation($"Movie '{movie.title}' added with ID: {movie.id}");

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing request: {ex.Message}");
                return new BadRequestObjectResult($"Error: {ex.Message}");
            }
        }
    }

}

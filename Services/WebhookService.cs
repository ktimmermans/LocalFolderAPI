using Microsoft.Extensions.Logging;
using Services.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Services
{
    public class WebhookService : IWebhookService
    {
        private readonly ILogger<WebhookService> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public WebhookService(
            ILogger<WebhookService> logger,
            IHttpClientFactory clientFactory)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
        }

        /// <summary>
        /// Function to send a file to an api endpoint
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileSpec">The filespec containing name and extension of the file</param>
        /// <param name="apiUrl">The endpoint to send the request to</param>
        /// <returns>Nothing</returns>
        public async Task SendFileToAPI(Stream file, FileSpec fileSpec, string apiUrl)
        {
            this._logger.LogInformation($"Sending file: {fileSpec.FileName}{fileSpec.Extension} to api: {apiUrl}");
            var client = this.CreateClient();

            // Prepare post parameters
            var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(file), "file", $"{fileSpec.FileName}{fileSpec.Extension}");

            // Prepare request
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("accept", "application/json");
            request.Content = formData;

            // Send request
            var result = await client.SendAsync(request);
            var resultMessage = await result.Content.ReadAsStreamAsync();

            // Process response
            if (!result.IsSuccessStatusCode)
            {
                var msg = new StreamReader(resultMessage).ReadToEnd();
                this._logger.LogError($"Sending file: {fileSpec.FileName}{fileSpec.Extension} to api: {apiUrl} failed with: {msg}");
                throw new Exception($"Posting file to: {apiUrl} failed with: {resultMessage}");
            }

        }

        /// <summary>
        /// Method to create a httpClient. Prepared for extensions with authentication for example
        /// </summary>
        /// <returns>A Httpclient prepared to do requests as needed</returns>
        private HttpClient CreateClient()
        {
            var client = this._clientFactory.CreateClient();

            return client;
        }
    }
}

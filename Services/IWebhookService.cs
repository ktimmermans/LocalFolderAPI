﻿using Services.Models;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
    public interface IWebhookService
    {
        /// <summary>
        /// Function to send a file to an api endpoint
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileSpec">The filespec containing name and extension of the file</param>
        /// <param name="apiUrl">The endpoint to send the request to</param>
        /// <returns>Nothing</returns>
        Task SendFileToAPI(Stream file, FileSpec fileSpec, string apiUrl);
    }
}

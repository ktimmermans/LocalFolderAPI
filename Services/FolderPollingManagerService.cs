using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.DTO;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class FolderPollingManagerService : IFolderPollingManagerService
    {
        private readonly IFolderService _folderService;
        private readonly ILogger<FolderPollingManagerService> _logger;
        private readonly IWebhookService _webhookService;

        public FolderPollingManagerService(
            IFolderService folderService,
            ILogger<FolderPollingManagerService> logger,
            IWebhookService webhookService
            )
        {
            this._folderService = folderService;
            this._logger = logger;
            this._webhookService = webhookService;
        }

        /// <summary>
        /// Function to create a task that polls a folder and processes the files it finds one by one
        /// - While processing posts to the configured webhook for the folder
        /// </summary>
        /// <param name="folderConfig">The folderconfiguration for the folder to poll</param>
        /// <returns>Nothing</returns>
        public async Task CreatePollingTaskForFolder(FolderConfig folderConfig)
        {
            var task = Task.Factory.StartNew(async () =>
            {
                this._logger.LogInformation($"Listing files for folder {folderConfig.FolderName}");

                var files = this._folderService.GetAllFilesForFolder(folderConfig.FolderName);
                foreach (var file in files)
                {
                    var fileSpec = new FileSpec
                    {
                        Extension = Path.GetExtension(file),
                        FileName = Path.GetFileNameWithoutExtension(file),
                    };
                    this._logger.LogInformation($"Found file: {fileSpec.FileName}.{fileSpec.Extension}");
                    using (var fileStream = File.OpenRead(file))
                    {
                        this._logger.LogInformation($"Processing webhook to post file: {fileSpec.FileName}{fileSpec.Extension} for folder: {folderConfig.FolderName}");
                        await this._webhookService.SendFileToAPI(fileStream, fileSpec, folderConfig.ApiUrl);
                    }

                    ProcessFile(file, folderConfig.PollingType.Value, folderConfig.MoveToFolder);
                }
            });
        }

        /// <summary>
        /// Function that processes a given file according to the given pollingtype
        /// </summary>
        /// <param name="file">the file to process</param>
        /// <param name="pollingType">Type of polling which determines what to do with the file</param>
        /// <param name="destinationFolder">The optional destinationfolder in case of MoveAfterFind</param>
        /// <returns>Nothing</returns>
        private void ProcessFile(string file, PollingType pollingType, string destinationFolder = null)
        {
            try
            {
                if (pollingType == PollingType.DeleteAfterFind)
                {
                    this._logger.LogInformation($"Deleting file: {file}");
                    File.Delete(file);
                }
                else if (pollingType == PollingType.MoveAfterFind)
                {
                    if (string.IsNullOrEmpty(destinationFolder))
                    {
                        throw new ArgumentException($"File cannot be moved because there is no destination folder configured");
                    }
                    var rootPath = Path.GetDirectoryName(file);
                    var filename = Path.GetFileName(file);
                    var destinationPath = Path.Combine(rootPath, destinationFolder);
                    var destinationFile = Path.Combine(destinationPath, filename);
                    this.CreateDirectoryIfNotExists(destinationPath);
                    this._logger.LogInformation($"Moving file: {file} to: {destinationFile}");
                    File.Move(file, destinationFile);
                }
                else
                {
                    throw new ArgumentException($"PollingType unknown.. Processing impossible for file: {file}");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Processing file: {file} failed because: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Function to check for a directory and create it if it doesnt exist
        /// </summary>
        /// <param name="directoryToCreate">The directory to check</param>
        /// <return>Nothing</return>
        private void CreateDirectoryIfNotExists(string directoryToCreate)
        {
            if (!Directory.Exists(directoryToCreate))
            {
                Directory.CreateDirectory(directoryToCreate);
            }
        }
    }
}

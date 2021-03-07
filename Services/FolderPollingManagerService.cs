using Microsoft.Extensions.Logging;
using Services.Models;
using System;
using System.IO;
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
            this._logger.LogInformation($"Listing files for folder {folderConfig.FolderName}");

            var files = await this._folderService.GetAllFilesForFolder(folderConfig.FolderName);
            foreach (var file in files)
            {
                await SendFileToApi(file, folderConfig);

                ProcessFile(file, folderConfig.PollingType, folderConfig.MoveToFolder);
            }
        }

        /// <summary>
        /// Send file to api by creating a filespec object and creating a http post to the API
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folderConfig"></param>
        /// <returns>Nothing</returns>
        private async Task SendFileToApi(string file, FolderConfig folderConfig)
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
        }

        /// <summary>
        /// Function that processes a given file according to the given pollingtype
        /// </summary>
        /// <param name="file">the file to process</param>
        /// <param name="pollingType">Type of polling which determines what to do with the file</param>
        /// <param name="destinationFolder">The optional destinationfolder in case of MoveAfterFind</param>
        /// <returns>Nothing</returns>
        private void ProcessFile(string file, string pollingType, string destinationFolder = null)
        {
            try
            {
                if (pollingType == PollingType.DeleteAfterFind.ToString())
                {
                    this._logger.LogInformation($"Deleting file: {file}");
                    File.Delete(file);
                }
                else if (pollingType == PollingType.MoveAfterFind.ToString())
                {
                    this.MoveFileToDestination(file, destinationFolder);
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
        /// Move a file to supplied destination folder while making sure the directory is created if it does not exist already
        /// </summary>
        /// <param name="currentFile">The path to the file to move</param>
        /// <param name="destinationFolder"></param>
        private void MoveFileToDestination(string currentFile, string destinationFolder)
        {
            var rootPath = Path.GetDirectoryName(currentFile);
            var filename = Path.GetFileName(currentFile);
            var destinationPath = Path.Combine(rootPath, destinationFolder);

            // make sure destination exists
            this.CreateDirectoryIfNotExists(destinationPath);

            // move file
            var destinationFile = Path.Combine(destinationPath, filename);
            this._logger.LogInformation($"Moving file: {currentFile} to: {destinationFile}");
            File.Move(currentFile, destinationFile);
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

using Glitter.BackgroundWorker;
using Glitter.BackgroundWorker.Interfaces;
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

        public FolderPollingManagerService(
            IFolderService folderService,
            ILogger<FolderPollingManagerService> logger
            )
        {
            this._folderService = folderService;
            this._logger = logger;
        }

        public async Task CreatePollingTaskForFolder(FolderConfig folderConfig)
        {
            var task = Task.Factory.StartNew(() =>
            {
                this._logger.LogInformation($"Listing files for folder {folderConfig.FolderName}");

                var files = this._folderService.GetAllFilesForFolder(folderConfig.FolderName);
                foreach (var file in files)
                {
                    var fileSpec = new FileSpec
                    {
                        Extension = Path.GetExtension(file),
                        FileName = Path.GetFileName(file),
                    };
                    this._logger.LogInformation($"Found file: {fileSpec.FileName}.{fileSpec.Extension}");
                    ProcessFile(file, folderConfig.PollingType, folderConfig.MoveToFolder);
                }
            });
        }

        private void ProcessFile(string file, PollingType pollingType, string destinationFolder = null)
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

        private void CreateDirectoryIfNotExists(string directoryToCreate)
        {
            if (!Directory.Exists(directoryToCreate))
            {
                Directory.CreateDirectory(directoryToCreate);
            }
        }
    }
}

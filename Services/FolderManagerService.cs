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
    public class FolderManagerService : IFolderManagerService
    {
        private readonly ILogger<FolderManagerService> _logger;
        private readonly IFolderPollingManagerService _folderPollingManager;
        private readonly IFolderConfigService _folderConfigService;
        private readonly IFolderService _folderService;

        public FolderManagerService(
            IFolderPollingManagerService folderPollingManager,
            IFolderConfigService folderConfigService,
            IFolderService folderService,
            ILogger<FolderManagerService> logger
            )
        {
            this._folderPollingManager = folderPollingManager;
            this._folderConfigService = folderConfigService;
            this._folderService = folderService;
            this._logger = logger;
        }

        public IEnumerable<FolderConfig> GetAllConfiguredFolders()
        {
            var configs = this._folderConfigService.GetAllConfiguredFolders();

            return configs;
        }

        public IEnumerable<FolderConfig> GetAllFoldersToPoll()
        {
            var configs = this._folderConfigService.GetAllConfiguredFolders();

            var configsToPoll = configs.Where(x => x.Polling);

            return configsToPoll;
        }

        public IEnumerable<string> GetAllFilesForFolder(string folderName)
        {
            var fileList = this._folderService.GetAllFilesForFolder(folderName);

            return fileList;
        }


        public void AddFolder(FolderConfig folderConfig)
        {
            this._folderConfigService.AddFolderToConfigIni(folderConfig);
        }

        public async Task AddFileToFolder(string folderName, Stream file, FileSpec fileSpecifications)
        {
            var fileName = $"{fileSpecifications.FileName}.{fileSpecifications.Extension}";

            await this._folderService.AddFileToFolder(folderName, file, fileName);
        }
    }
}

using BackgroundWorker.TaskManager;
using Services.FolderConfiguration;
using Services.Interfaces;
using Services.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class FolderManagerService : IFolderManagerService
    {
        private readonly IFolderConfigService _folderConfigService;
        private readonly IFolderService _folderService;
        private readonly IBackgroundTaskManager _taskManager;

        public FolderManagerService(
            IFolderConfigService folderConfigService,
            IFolderService folderService,
            IBackgroundTaskManager taskManager
            )
        {
            this._folderConfigService = folderConfigService;
            this._folderService = folderService;
            this._taskManager = taskManager;
        }

        public async Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders()
        {
            var configs = await this._folderConfigService.GetAllConfiguredFolders();

            return configs;
        }

        public async Task<IEnumerable<FolderConfig>> GetAllFoldersToPoll()
        {
            var configs = await this._folderConfigService.GetAllConfiguredFolders();

            var configsToPoll = configs.Where(x => x.Polling);

            return configsToPoll;
        }

        public async Task<IEnumerable<string>> GetAllFilesForFolder(string folderName)
        {
            var fileList = await this._folderService.GetAllFilesForFolder(folderName);

            return fileList;
        }

        public void AddFolder(FolderConfig folderConfig)
        {
            this._folderConfigService.AddFolder(folderConfig);
            if (folderConfig.Polling)
            {
                this._taskManager.AddTask(new PollingTaskDescriptor
                {
                    DelayMilliSeconds = 10000, // Start after 10 seconds
                    FolderName = folderConfig.FolderName,
                });
            }
        }

        public async Task<FolderConfig> GetFolderConfigByFolderName(string folderName)
        {
            var folder = await this._folderConfigService.GetFolderConfigByFolderName(folderName);

            return folder;
        }

        public void UpdateFolder(FolderConfig folderConfigNew, string folderNameOld)
        {
            this._folderConfigService.UpdateFolder(folderConfigNew, folderNameOld);
        }

        public void DeleteFolder(FolderConfig folderConfig)
        {
            this._folderConfigService.DeleteFolder(folderConfig.FolderName);
        }

        public async Task AddFileToFolder(string folderName, Stream file, FileSpec fileSpecifications)
        {
            var fileName = $"{fileSpecifications.FileName}.{fileSpecifications.Extension}";

            await this._folderService.AddFileToFolder(folderName, file, fileName);
        }
    }
}

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

        /// <summary>
        /// Get all currently configured folders
        /// </summary>
        /// <returns>A list of FolderConfig objects</returns>
        public async Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders()
        {
            var configs = await this._folderConfigService.GetAllConfiguredFolders();

            return configs;
        }

        /// <summary>
        /// Get a list of all configured folders that have a polling option enabled
        /// </summary>
        /// <returns>A list of FolderConfig objects for folders that have a polling option enabled</returns>
        public async Task<IEnumerable<FolderConfig>> GetAllFoldersToPoll()
        {
            var configs = await this._folderConfigService.GetAllConfiguredFolders();

            var configsToPoll = configs.Where(x => x.Polling);

            return configsToPoll;
        }

        /// <summary>
        /// Get a list of all absolute paths to files in a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A list of absolute paths to files in the folder</returns>
        public async Task<IEnumerable<string>> GetAllFilesForFolder(string folderName)
        {
            var fileList = await this._folderService.GetAllFilesForFolder(folderName);

            return fileList;
        }

        /// <summary>
        /// Add a new folderconfiguration to the config
        /// </summary>
        /// <param name="folderConfig"></param>
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

        /// <summary>
        /// Get a FolderConfig by a foldername
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A FolderConfig object</returns>
        public async Task<FolderConfig> GetFolderConfigByFolderName(string folderName)
        {
            var folder = await this._folderConfigService.GetFolderConfigByFolderName(folderName);

            return folder;
        }

        /// <summary>
        /// Update a FolderConfig
        /// </summary>
        /// <param name="folderConfigNew">The new Folderconfig</param>
        /// <param name="folderNameOld">The foldername as it is now</param>
        public void UpdateFolder(FolderConfig folderConfigNew, string folderNameOld)
        {
            this._folderConfigService.UpdateFolder(folderConfigNew, folderNameOld);
        }

        /// <summary>
        /// Remove a FolderConfig from the configuration
        /// </summary>
        /// <param name="folderConfig"></param>
        public void DeleteFolder(FolderConfig folderConfig)
        {
            this._folderConfigService.DeleteFolder(folderConfig.FolderName);
        }

        /// <summary>
        /// Add a file to a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="file"></param>
        /// <param name="fileSpecifications"></param>
        /// <returns>Nothing</returns>
        public async Task AddFileToFolder(string folderName, Stream file, FileSpec fileSpecifications)
        {
            var fileName = $"{fileSpecifications.FileName}.{fileSpecifications.Extension}";

            await this._folderService.AddFileToFolder(folderName, file, fileName);
        }
    }
}

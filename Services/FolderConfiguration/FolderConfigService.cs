using Microsoft.EntityFrameworkCore;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.FolderConfiguration
{
    public class FolderConfigService : IFolderConfigService
    {
        private readonly IFolderConfigurationProvider _configProvider;
        private readonly ProjectContext _dbContext;
        public FolderConfigService(
            IFolderConfigurationProvider configProvider,
            ProjectContext dbContext
            )
        {
            this._configProvider = configProvider;
            this._dbContext = dbContext;

            this.Initialize().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get all FolderConfigs for the current configuration
        /// </summary>
        /// <returns>A list of FolderConfig objects</returns>
        public async Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders()
        {
            var folders = await this._dbContext.Set<FolderConfig>().ToListAsync();

            return folders;
        }

        /// <summary>
        /// Get the entire GlobalConfig
        /// </summary>
        /// <returns>GlobalConfig file if it exists, otherwise throws Exception</returns>
        public async Task<GlobalConfig> GetGlobalConfig()
        {
            var globalConfig = await this._dbContext.Set<GlobalConfig>().FirstOrDefaultAsync();

            if (globalConfig == null)
            {
                throw new Exception($"Global configuration does not exist in the config");
            }

            return globalConfig;
        }

        /// <summary>
        /// Read GlobalConfig and return the default polling interval
        /// </summary>
        /// <returns>An interval in seconds</returns>
        public async Task<int> GetPollingInterval()
        {
            var globalConfig = await this.GetGlobalConfig();

            return globalConfig.PollingInterval;
        }

        /// <summary>
        /// Check if a FolderConfig exists for the given foldername
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>True if it exists, else False</returns>
        public async Task<bool> FolderExists(string folderName)
        {
            var folder = await this._dbContext.Set<FolderConfig>().FirstOrDefaultAsync(x => x.FolderName == folderName);

            return folder != null;
        }

        /// <summary>
        /// Retreive a specific FolderConfig object from the configuration
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A FolderConfig object if it exists, otherwise throws exception</returns>
        public async Task<FolderConfig> GetFolderConfigByFolderName(string folderName)
        {
            var folder = await this._dbContext.Set<FolderConfig>().FirstOrDefaultAsync(x => x.FolderName == folderName);

            if (folder == null)
            {
                throw new Exception($"Folder: {folderName} does not exist in the config");
            }

            return folder;
        }

        /// <summary>
        /// Remove a FolderConfig from the current configuration, also saves the configuration file
        /// </summary>
        /// <param name="folderNameToDelete"></param>
        /// <returns>Nothing</returns>
        public async Task DeleteFolder(string folderNameToDelete)
        {
            var folder = await this.GetFolderConfigByFolderName(folderNameToDelete);

            this._dbContext.Set<FolderConfig>().Remove(folder);

            await this._dbContext.SaveChangesAsync();
            await this.SaveConfig();
        }

        /// <summary>
        /// Add a new FolderConfig to the current configuration, also saves it to the configuration file
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <returns>Nothing</returns>
        public async Task AddFolder(FolderConfig folderConfig)
        {
            // No recursive polling and Move files since it can create an endless loop
            if (folderConfig.IsRecursive && folderConfig.PollingType != PollingType.MoveAfterFind.ToString())
            {
                throw new Exception($"Files can not be moved to another direction when recursively polling for folder: {folderConfig.FolderName}");
            }

            // No adding folders with the same name
            if (await this.FolderExists(folderConfig.FolderName))
            {
                throw new Exception($"Folder: {folderConfig.FolderName} already exist in the config");
            }

            this._dbContext.Set<FolderConfig>().Add(folderConfig);
            await this._dbContext.SaveChangesAsync();
            await this.SaveConfig();
        }

        /// <summary>
        /// Update a FolderConfig in the current configuration, also saves it to the configuration file
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <param name="oldFolderName"></param>
        /// <returns>Nothing</returns>
        public async Task UpdateFolder(FolderConfig folderConfig, string oldFolderName)
        {
            var folder = await this.GetFolderConfigByFolderName(oldFolderName);

            folder.Path = folderConfig.Path;
            folder.Polling = folderConfig.Polling;
            folder.FolderName = folderConfig.FolderName;

            if (folderConfig.Polling)
            {
                folder.ApiUrl = folderConfig.ApiUrl;
                folder.MoveToFolder = folderConfig.MoveToFolder;
                folder.PollingType = folderConfig.PollingType;
            }
            else
            {
                folder.ApiUrl = null;
                folder.MoveToFolder = null;
                folder.PollingType = null;
            }
            await this._dbContext.SaveChangesAsync();
            await this.SaveConfig();
        }

        /// <summary>
        /// Try to fill the database initally, if it was already configured does nothing
        /// </summary>
        /// <returns>Nothing</returns>
        private async Task Initialize()
        {
            var existingConfig = await this._dbContext.Set<GlobalConfig>().FirstOrDefaultAsync();
            if (existingConfig == null)
            {
                var globalConfig = this._configProvider.GetGlobalConfiguration();

                this._dbContext.Set<GlobalConfig>().Add(globalConfig);
                this._dbContext.Set<FolderConfig>().AddRange(globalConfig.FolderConfigs);
                await this._dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Save the configuration file
        /// </summary>
        /// <returns>Nothing</returns>
        private async Task SaveConfig()
        {
            var config = await this.GetGlobalConfig();
            var folders = await this.GetAllConfiguredFolders();
            config.FolderConfigs = folders.ToList();

            this._configProvider.SetGlobalConfiguration(config);
        }
    }
}

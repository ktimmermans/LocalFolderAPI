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

        public async Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders()
        {
            var folders = await this._dbContext.Set<FolderConfig>().ToListAsync();

            return folders;
        }

        public async Task<GlobalConfig> GetGlobalConfig()
        {
            var globalConfig = await this._dbContext.Set<GlobalConfig>().FirstOrDefaultAsync();

            if (globalConfig == null)
            {
                throw new Exception($"Global configuration does not exist in the config");
            }

            return globalConfig;
        }

        public async Task<int> GetPollingInterval()
        {
            var globalConfig = await this.GetGlobalConfig();

            return globalConfig.PollingInterval;
        }

        public async Task<bool> FolderExists(string folderName)
        {
            var folder = await this._dbContext.Set<FolderConfig>().FirstOrDefaultAsync(x => x.FolderName == folderName);

            return folder != null;
        }

        public async Task<FolderConfig> GetFolderConfigByFolderName(string folderName)
        {
            var folder = await this._dbContext.Set<FolderConfig>().FirstOrDefaultAsync(x => x.FolderName == folderName);

            if (folder == null)
            {
                throw new Exception($"Folder: {folderName} does not exist in the config");
            }

            return folder;
        }

        public async Task DeleteFolder(string folderNameToDelete)
        {
            var folder = await this.GetFolderConfigByFolderName(folderNameToDelete);

            this._dbContext.Set<FolderConfig>().Remove(folder);

            await this._dbContext.SaveChangesAsync();
            await this.SaveConfig();
        }

        public async Task AddFolder(FolderConfig folderConfig)
        {
            if (await this.FolderExists(folderConfig.FolderName))
            {
                throw new Exception($"Folder: {folderConfig.FolderName} already exist in the config");
            }

            this._dbContext.Set<FolderConfig>().Add(folderConfig);
            await this._dbContext.SaveChangesAsync();
            await this.SaveConfig();
        }

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

        private async Task SaveConfig()
        {
            var config = await this.GetGlobalConfig();
            var folders = await this.GetAllConfiguredFolders();
            config.FolderConfigs = folders.ToList();

            this._configProvider.SetGlobalConfiguration(config);
        }
    }
}

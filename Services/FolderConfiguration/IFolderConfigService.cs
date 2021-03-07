using Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.FolderConfiguration
{
    public interface IFolderConfigService
    {
        Task<FolderConfig> GetFolderConfigByFolderName(string folderName);

        Task<bool> FolderExists(string folderName);

        Task<int> GetPollingInterval();

        Task<GlobalConfig> GetGlobalConfig();

        Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders();

        Task AddFolder(FolderConfig folderConfig);

        Task UpdateFolder(FolderConfig folderConfig, string oldFolderName);

        Task DeleteFolder(string folderNameToDelete);
    }
}

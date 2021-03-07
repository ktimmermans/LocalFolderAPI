using Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.FolderConfiguration
{
    public interface IFolderConfigService
    {
        /// <summary>
        /// Retreive a specific FolderConfig object from the configuration
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A FolderConfig object if it exists, otherwise throws exception</returns>
        Task<FolderConfig> GetFolderConfigByFolderName(string folderName);

        /// <summary>
        /// Check if a FolderConfig exists for the given foldername
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>True if it exists, else False</returns>
        Task<bool> FolderExists(string folderName);

        /// <summary>
        /// Read GlobalConfig and return the default polling interval
        /// </summary>
        /// <returns>An interval in seconds</returns>
        Task<int> GetPollingInterval();

        /// <summary>
        /// Get the entire GlobalConfig
        /// </summary>
        /// <returns>GlobalConfig file if it exists, otherwise throws Exception</returns>
        Task<GlobalConfig> GetGlobalConfig();

        /// <summary>
        /// Get all FolderConfigs for the current configuration
        /// </summary>
        /// <returns>A list of FolderConfig objects</returns>
        Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders();

        /// <summary>
        /// Add a new FolderConfig to the current configuration, also saves it to the configuration file
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <returns>Nothing</returns>
        Task AddFolder(FolderConfig folderConfig);

        /// <summary>
        /// Update a FolderConfig in the current configuration, also saves it to the configuration file
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <param name="oldFolderName"></param>
        /// <returns>Nothing</returns>
        Task UpdateFolder(FolderConfig folderConfig, string oldFolderName);

        /// <summary>
        /// Remove a FolderConfig from the current configuration, also saves the configuration file
        /// </summary>
        /// <param name="folderNameToDelete"></param>
        /// <returns>Nothing</returns>
        Task DeleteFolder(string folderNameToDelete);
    }
}

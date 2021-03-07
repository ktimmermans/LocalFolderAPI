using Services.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFolderManagerService
    {
        /// <summary>
        /// Get all currently configured folders
        /// </summary>
        /// <returns>A list of FolderConfig objects</returns>
        Task<IEnumerable<FolderConfig>> GetAllConfiguredFolders();

        /// <summary>
        /// Add a new folderconfiguration to the config
        /// </summary>
        /// <param name="folderConfig"></param>
        void AddFolder(FolderConfig folderConfig);

        /// <summary>
        /// Get a FolderConfig by a foldername
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A FolderConfig object</returns>
        Task<FolderConfig> GetFolderConfigByFolderName(string folderName);

        /// <summary>
        /// Update a FolderConfig
        /// </summary>
        /// <param name="folderConfigNew">The new Folderconfig</param>
        /// <param name="folderNameOld">The foldername as it is now</param>
        void UpdateFolder(FolderConfig folderConfigNew, string folderNameOld);

        /// <summary>
        /// Remove a FolderConfig from the configuration
        /// </summary>
        /// <param name="folderConfig"></param>
        void DeleteFolder(FolderConfig folderConfig);

        /// <summary>
        /// Get a list of all absolute paths to files in a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A list of absolute paths to files in the folder</returns>
        Task<IEnumerable<string>> GetAllFilesForFolder(string folderName);

        Task AddFileToFolder(string folderName, Stream file, FileSpec fileSpecifications);

        /// <summary>
        /// Get a list of all configured folders that have a polling option enabled
        /// </summary>
        /// <returns>A list of FolderConfig objects for folders that have a polling option enabled</returns>
        Task<IEnumerable<FolderConfig>> GetAllFoldersToPoll();
    }
}

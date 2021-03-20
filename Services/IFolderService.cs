using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
    public interface IFolderService
    {
        /// <summary>
        /// Get a list of all directories in a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>A list of directory names for the folder supplied</returns>
        IEnumerable<string> GetDirectoriesInFolder(string folder);

        /// <summary>
        /// Get a list of files from a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="recursive">toggle recursive file retreival</param>
        /// <returns>A list of absolute paths to files in a folder</returns>
        Task<IEnumerable<string>> GetAllFilesForFolder(string folderName, bool recursive = false);

        /// <summary>
        /// Add a file to supplied folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <returns>Nothing</returns>
        Task AddFileToFolder(string folderName, Stream file, string fileName);
    }
}

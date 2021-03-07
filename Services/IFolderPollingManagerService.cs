using Services.Models;
using System.Threading.Tasks;

namespace Services
{
    public interface IFolderPollingManagerService
    {
        /// <summary>
        /// Function to create a task that polls a folder and processes the files it finds one by one
        /// - While processing posts to the configured webhook for the folder
        /// </summary>
        /// <param name="folderConfig">The folderconfiguration for the folder to poll</param>
        /// <returns>Nothing</returns>
        Task CreatePollingTaskForFolder(FolderConfig folderConfig);
    }
}

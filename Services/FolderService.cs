using Services.FolderConfiguration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderConfigService _folderConfigService;

        public FolderService(
            IFolderConfigService folderConfigService)
        {
            this._folderConfigService = folderConfigService;
        }

        /// <summary>
        /// Get a list of all directories in a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>A list of directory names for the folder supplied</returns>
        public IEnumerable<string> GetDirectoriesInFolder(string folder)
        {
            var dirInfo = new DirectoryInfo($@"{folder}");
            var dirList = dirInfo.GetDirectories().Select(x => x.FullName);

            return dirList;
        }

        /// <summary>
        /// Get a list of files from a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>A list of absolute paths to files in a folder</returns>
        public async Task<IEnumerable<string>> GetAllFilesForFolder(string folderName, bool recursive = false)
        {
            var folder = await this._folderConfigService.GetFolderConfigByFolderName(folderName);
            List<string> fileList = new List<string>();

            var dirInfo = new DirectoryInfo($@"{folder.Path}");
            fileList.AddRange(dirInfo.GetFiles().Select(x => x.FullName));

            // Recursive 1 layer
            foreach (var dir in dirInfo.GetDirectories())
            {
                var files = dir.GetFiles().Select(x => x.FullName);
                fileList.AddRange(files);
            }

            return fileList;
        }

        /// <summary>
        /// Add a file to supplied folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <returns>Nothing</returns>
        public async Task AddFileToFolder(string folderName, Stream file, string fileName)
        {
            var folder = await this._folderConfigService.GetFolderConfigByFolderName(folderName);
            var filePath = Path.Combine(folder.Path, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                await file.CopyToAsync(fs);
            }
        }
    }
}

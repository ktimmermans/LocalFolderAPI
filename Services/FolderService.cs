using Microsoft.Extensions.Logging;
using Services.FolderConfiguration;
using Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class FolderService : IFolderService
    {
        private readonly ILogger<FolderService> _logger;
        private readonly IFolderConfigService _folderConfigService;

        public FolderService(
            ILogger<FolderService> logger,
            IFolderConfigService folderConfigService)
        {
            this._logger = logger;
            this._folderConfigService = folderConfigService;
        }


        public IEnumerable<string> GetDirectoriesInFolder(string folder)
        {
            var dirInfo = new DirectoryInfo($@"{folder}");
            var dirList = dirInfo.GetDirectories().Select(x => x.FullName);


            return dirList;
        }

        public async Task<IEnumerable<string>> GetAllFilesForFolder(string folderName)
        {
            var folder = await this._folderConfigService.GetFolderConfigByFolderName(folderName);

            var dirInfo = new DirectoryInfo($@"{folder.Path}");
            var fileList = dirInfo.GetFiles().Select(x => x.FullName);

            return fileList;
        }

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

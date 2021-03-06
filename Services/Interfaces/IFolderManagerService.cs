using Services.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFolderManagerService
    {
        IEnumerable<FolderConfig> GetAllConfiguredFolders();

        void AddFolder(FolderConfig folderConfig);

        FolderConfig GetFolderConfigByFolderName(string folderName);

        void UpdateFolder(FolderConfig folderConfigNew, string folderNameOld);

        void DeleteFolder(FolderConfig folderConfig);

        IEnumerable<string> GetAllFilesForFolder(string folderName);

        Task AddFileToFolder(string folderName, Stream file, FileSpec fileSpecifications);

        IEnumerable<FolderConfig> GetAllFoldersToPoll();
    }
}

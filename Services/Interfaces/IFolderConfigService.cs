using Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFolderConfigService
    {
        IEnumerable<FolderConfig> GetAllConfiguredFolders();

        string GetPathForFolder(string folderName);

        int GetPollingDelayForFolder(string folderName);

        void AddFolderToConfigIni(FolderConfig folderConfig);

        void RemoveFolderFromConfigIni(FolderConfig folderConfig);
    }
}

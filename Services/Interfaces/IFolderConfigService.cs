using Services.DTO;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface IFolderConfigService
    {
        IEnumerable<FolderConfig> GetAllConfiguredFolders();

        FolderConfig GetFolderConfigByFolderName(string folderName);

        string GetPathForFolder(string folderName);

        int GetPollingDelayForFolder(string folderName);

        void AddFolderToConfigIni(FolderConfig folderConfig);
        
        void UpdateFolderFromConfigIni(FolderConfig folderConfigNew, string folderNameOld);

        void RemoveFolderFromConfigIni(FolderConfig folderConfig);
    }
}

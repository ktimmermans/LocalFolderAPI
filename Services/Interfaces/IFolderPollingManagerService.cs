using Services.Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFolderPollingManagerService
    {
        Task CreatePollingTaskForFolder(FolderConfig folderConfig);
    }
}

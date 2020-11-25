using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFolderService
    {
        IEnumerable<string> GetDirectoriesInFolder(string folder);

        IEnumerable<string> GetAllFilesForFolder(string folderName);

        Task AddFileToFolder(string folderName, Stream file, string fileName);
    }
}

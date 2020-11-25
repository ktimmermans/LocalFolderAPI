using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO
{
    public class FolderConfig
    {
        public string Path { get; set; }

        public string FolderName { get; set; }

        public bool Polling { get; set; }

        public PollingType PollingType { get; set; }

        public string MoveToFolder { get; set; }

        public string ApiUrl { get; set; }
    }
}

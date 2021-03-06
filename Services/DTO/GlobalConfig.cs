using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO
{
    public class GlobalConfig
    {
        public int PollingInterval { get; set; }

        public List<FolderConfig> FolderConfigs { get; set; }
    }
}

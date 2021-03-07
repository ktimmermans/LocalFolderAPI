using System.Collections.Generic;

namespace Services.Models
{
    public class GlobalConfig
    {
        public string InstanceName { get; set; }
        public int PollingInterval { get; set; }

        public List<FolderConfig> FolderConfigs { get; set; }
    }
}

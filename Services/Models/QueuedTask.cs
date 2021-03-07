using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class QueuedTask
    {
        public string Name { get; set; }

        public DateTime NextRun { get; set; }

        public string FolderName { get; set; }
    }
}

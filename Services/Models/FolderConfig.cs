﻿namespace Services.Models
{
    public class FolderConfig
    {
        public string Path { get; set; }

        public string FolderName { get; set; }

        public bool Polling { get; set; }

        public string PollingType { get; set; }

        public string MoveToFolder { get; set; }

        public string ApiUrl { get; set; }
    }
}
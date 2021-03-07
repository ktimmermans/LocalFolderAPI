using BackgroundWorker.Abstractions;

namespace Services
{
    public class PollingTaskDescriptor : TaskSettings
    {
        public string FolderName { get; set; }
        public override int DelayMilliSeconds { get; set; }
        public override string TaskName { get; set; } = "FolderPollingTask";
    }
}

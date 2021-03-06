using Background;

namespace Services
{
    public class PollingTaskDescriptor : TaskSettings
    {
        public override int DelayMilliSeconds { get; set; }
        public override string TaskName { get; set; }
    }
}

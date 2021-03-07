using Services.Models;

namespace Services.FolderConfiguration
{
    public interface IFolderConfigurationProvider
    {
        void SetGlobalConfiguration(GlobalConfig globalConfig);

        GlobalConfig GetGlobalConfiguration();
    }
}

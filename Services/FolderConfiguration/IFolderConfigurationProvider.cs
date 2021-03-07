using Services.Models;

namespace Services.FolderConfiguration
{
    public interface IFolderConfigurationProvider
    {
        /// <summary>
        /// Save representation to the default location with the current GlobalConfig
        /// </summary>
        /// <param name="globalConfig"></param>
        void SetGlobalConfiguration(GlobalConfig globalConfig);

        /// <summary>
        /// Read the default location for a configuration file and return the GlobalConfig object
        /// </summary>
        /// <returns>GlobalConfig object containing all configuration information</returns>
        GlobalConfig GetGlobalConfiguration();
    }
}

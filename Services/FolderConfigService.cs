using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;
using Services.DTO;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services
{
    public class FolderConfigService : IFolderConfigService
    {
        private readonly ILogger<FolderConfigService> _logger;
        private readonly string _rootDir;
        private readonly string _iniPath;

        private const string CONFIG_FILE = "FolderConfiguration.ini";

        public FolderConfigService(
            ILogger<FolderConfigService> logger)
        {
            this._logger = logger;
            this._rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FolderPoller");
            // Set desired ini path
            this._iniPath = Path.Combine(this._rootDir, CONFIG_FILE);

            this.GetIniDateOrCreateFile();
        }

        public IEnumerable<FolderConfig> GetAllConfiguredFolders()
        {
            var globalConfig = this.ReadConfigIni();

            return globalConfig.FolderConfigs;
        }

        public int GetPollingInterval()
        {
            var globalConfig = this.ReadConfigIni();

            return globalConfig.PollingInterval;
        }

        private SectionData DoesFolderExistsInIni(string folderName)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();
            var folder = iniData.Sections.FirstOrDefault(x => x.SectionName == folderName);

            if (folder == null)
            {
                throw new Exception($"Folder: {folderName} does not exist in the config");
            }

            return folder;
        }

        public FolderConfig GetFolderConfigByFolderName(string folderName)
        {
            var folder = DoesFolderExistsInIni(folderName);

            bool isPolling = bool.Parse(folder.Keys.GetKeyData("IsPolling").Value);

            var folderConfig = new FolderConfig
            {
                Path = folder.Keys.GetKeyData("Path").Value,
                FolderName = folder.SectionName,
            };

            if (isPolling)
            {
                folderConfig.PollingType = (PollingType)Enum
                    .Parse(typeof(PollingType), folder.Keys.GetKeyData("PollingType").Value);
                folderConfig.MoveToFolder = folder.Keys.GetKeyData("DestinationFolder").Value;
                folderConfig.ApiUrl = folder.Keys.GetKeyData("ApiUrl").Value;
                folderConfig.Polling = isPolling;

            }

            return folderConfig;
        }

        public string GetPathForFolder(string folderName)
        {
            var folder = DoesFolderExistsInIni(folderName);

            return folder.Keys.First(x => x.KeyName == "Path").Value;
        }

        public int GetPollingDelayForFolder(string folderName)
        {
            var folder = DoesFolderExistsInIni(folderName);

            return int.Parse(folder.Keys.First(x => x.KeyName == "Delay").Value);
        }

        public void AddFolderToConfigIni(FolderConfig folderConfig)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();

            var folderNameNoSpace = Regex.Replace(folderConfig.FolderName, @"\s+", "");

            if (iniData.Sections.Any(x => x.SectionName == folderNameNoSpace))
            {
                throw new Exception($"Folder: {folderNameNoSpace} already exists");
            }

            var sectionKeys = new KeyDataCollection();
            sectionKeys.AddKey("Path", folderConfig.Path.Replace(@"\", "\\"));
            sectionKeys.AddKey("IsPolling", folderConfig.Polling.ToString());
            if (folderConfig.Polling)
            {
                sectionKeys.AddKey("DestinationFolder", folderConfig.MoveToFolder);
                sectionKeys.AddKey("PollingType", folderConfig.PollingType.ToString());
                sectionKeys.AddKey("ApiUrl", folderConfig.ApiUrl);
            }

            var data = new SectionData(folderNameNoSpace)
            {
                Keys = sectionKeys,
            };

            this._logger.LogInformation($"Adding {folderNameNoSpace} to ${CONFIG_FILE} at: {this._rootDir}");
            iniData.Sections.Add(data);

            parser.WriteFile(this._iniPath, iniData);
        }

        public void UpdateFolderFromConfigIni(FolderConfig folderConfigNew, string folderNameOld)
        {
            // Delete old add new

            var oldFolder = GetFolderConfigByFolderName(folderNameOld);
            RemoveFolderFromConfigIni(oldFolder);

            AddFolderToConfigIni(folderConfigNew);
        }

        public void RemoveFolderFromConfigIni(FolderConfig folderConfig)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();

            this._logger.LogInformation($"Removing {folderConfig.FolderName} from ${CONFIG_FILE} at: {this._rootDir}");
            iniData.Sections.RemoveSection(folderConfig.FolderName);

            parser.WriteFile(this._iniPath, iniData);
        }

        private GlobalConfig ReadConfigIni()
        {
            GlobalConfig globalConfig = new GlobalConfig();

            globalConfig.FolderConfigs = new List<FolderConfig>();
            var iniData = this.GetIniDateOrCreateFile();

            this._logger.LogInformation($"Found: {iniData.Sections.Count} folder configurations");
            foreach (var section in iniData.Sections)
            {
                var folderConfig = this.readFolderSection(section);

                globalConfig.FolderConfigs.Add(folderConfig);
            }

            globalConfig.PollingInterval = int.Parse(iniData.GetKey("PollingInterval"));

            return globalConfig;
        }

        private FolderConfig readFolderSection(SectionData section)
        {
            var pollingString = section.Keys.GetKeyData("IsPolling").Value;
            bool isPolling = bool.Parse(pollingString);

            var folderConfig = new FolderConfig
            {
                Polling = isPolling,
                FolderName = section.SectionName,
                Path = section.Keys.GetKeyData("Path").Value,
            };

            if (isPolling)
            {
                var pollingTypeString = section.Keys.GetKeyData("PollingType").Value;
                var pollingType = (PollingType)Enum.Parse(typeof(PollingType), pollingTypeString);
                folderConfig.PollingType = pollingType;

                var destinationFolder = section.Keys.GetKeyData("DestinationFolder").Value;
                folderConfig.MoveToFolder = destinationFolder;

                var folderAPiUrl = section.Keys.GetKeyData("ApiUrl").Value;
                folderConfig.ApiUrl = folderAPiUrl;
            }
            return folderConfig;
        }

        private IniData GetIniDateOrCreateFile()
        {
            this._logger.LogInformation($"Reading {CONFIG_FILE} ini at: {this._rootDir}");
            var parser = new FileIniDataParser();
            IniData data;

            // create directory
            if (!Directory.Exists(this._rootDir))
            {
                Directory.CreateDirectory(this._rootDir);
            }
            // Create file
            if (!File.Exists(this._iniPath))
            {
                this._logger.LogInformation($"Config file not found, writing new {CONFIG_FILE} ini to: {this._rootDir}");
                data = new IniData();
                // Add default 60seconds delay for polling
                data.Global.AddKey("PollingInterval", 60.ToString());
                // File does not exist, create it
                parser.WriteFile(this._iniPath, data);
            }
            else
            {
                data = parser.ReadFile(this._iniPath);
            }

            // Set default 60 seconds delay if it is missing
            if (string.IsNullOrEmpty(data.Global.GetKeyData("PollingInterval")?.Value))
            {
                data.Global.AddKey("PollingInterval", 60.ToString());
            }

            return data;
        }
    }
}

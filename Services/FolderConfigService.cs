using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;
using Services.DTO;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this._rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this._iniPath = Path.Combine(this._rootDir, CONFIG_FILE);
        }

        public IEnumerable<FolderConfig> GetAllConfiguredFolders()
        {
            var folders = this.ReadConfigIni();

            return folders;
        }

        public string GetPathForFolder(string folderName)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();
            var folder = iniData.Sections.FirstOrDefault(x => x.SectionName == folderName);

            if (folder == null)
            {
                throw new Exception($"Folder: {folderName} does not exist in the config");
            }

            return folder.Keys.First(x => x.KeyName == "Path").Value;
        }

        public int GetPollingDelayForFolder(string folderName)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();
            var folder = iniData.Sections.FirstOrDefault(x => x.SectionName == folderName);

            if (folder == null)
            {
                throw new Exception($"Folder: {folderName} does not exist in the config");
            }

            return int.Parse(folder.Keys.First(x => x.KeyName == "Delay").Value);
        }

        private IEnumerable<FolderConfig> ReadConfigIni()
        {
            List<FolderConfig> folders = new List<FolderConfig>();
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();

            this._logger.LogInformation($"Found: {iniData.Sections.Count} folder configurations");
            foreach (var section in iniData.Sections)
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

                folders.Add(folderConfig);
            }

            return folders;
        }

        public void AddFolderToConfigIni(FolderConfig folderConfig)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();

            if (iniData.Sections.Any(x => x.SectionName == folderConfig.FolderName))
            {
                throw new Exception($"Folder: {folderConfig.FolderName} already exists");
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

            var data = new SectionData(folderConfig.FolderName)
            {
                Keys = sectionKeys,
            };

            this._logger.LogInformation($"Adding {folderConfig.FolderName} to ${CONFIG_FILE} at: {this._rootDir}");
            iniData.Sections.Add(data);

            parser.WriteFile(this._iniPath, iniData);
        }

        public void RemoveFolderFromConfigIni(FolderConfig folderConfig)
        {
            var parser = new FileIniDataParser();
            var iniData = this.GetIniDateOrCreateFile();

            this._logger.LogInformation($"Removing {folderConfig.FolderName} from ${CONFIG_FILE} at: {this._rootDir}");
            iniData.Sections.RemoveSection(folderConfig.FolderName);

            parser.WriteFile(CONFIG_FILE, iniData);
        }

        private IniData GetIniDateOrCreateFile()
        {
            this._logger.LogInformation($"Reading {CONFIG_FILE} ini at: {this._rootDir}");
            var parser = new FileIniDataParser();
            IniData data;

            try
            {
                data = parser.ReadFile(this._iniPath);
            }
            catch (Exception ex)
            {
                this._logger.LogInformation($"{CONFIG_FILE} ini not found at: {this._rootDir}, creating one...");
                data = new IniData();
                // File does not exist, create it
                parser.WriteFile(this._iniPath, data);
            }

            return data;
        }
    }
}

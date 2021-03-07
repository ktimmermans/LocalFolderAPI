﻿using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;
using Services.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.FolderConfiguration
{
    public class IniConfigurationProvider : IFolderConfigurationProvider
    {
        private readonly ILogger<FolderConfigService> _logger;
        private readonly string _rootDir;
        private readonly string _iniPath;

        private const string CONFIG_FILE = "FolderConfiguration.ini";

        public IniConfigurationProvider(
            ILogger<FolderConfigService> logger)
        {
            this._logger = logger;
            this._rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FolderPoller");
            // Set desired ini path
            this._iniPath = Path.Combine(this._rootDir, CONFIG_FILE);
        }

        public void SetGlobalConfiguration(GlobalConfig globalConfig)
        {
            var parser = new FileIniDataParser();
            var iniData = new IniData();

            foreach (var folder in globalConfig.FolderConfigs)
            {
                var section = this.GetFolderSectionFromConfig(folder);
                iniData.Sections.Add(section);
            }

            parser.WriteFile(this._iniPath, iniData);
        }

        public GlobalConfig GetGlobalConfiguration()
        {
            var iniData = this.GetIniDataOrCreateFile();

            GlobalConfig globalConfig = new GlobalConfig();
            globalConfig.PollingInterval = int.Parse(iniData.GetKey("PollingInterval"));
            globalConfig.InstanceName = iniData.GetKey("InstanceName");

            globalConfig.FolderConfigs = new List<FolderConfig>();

            this._logger.LogInformation($"Found: {iniData.Sections.Count} folder configurations");
            foreach (var section in iniData.Sections)
            {
                var folderConfig = this.readFolderSection(section);

                globalConfig.FolderConfigs.Add(folderConfig);
            }

            return globalConfig;
        }

        private SectionData GetFolderSectionFromConfig(FolderConfig folderConfig)
        {
            var sectionData = new SectionData(folderConfig.FolderName);

            var sectionKeys = new KeyDataCollection();
            sectionKeys.AddKey("Path", folderConfig.Path.Replace(@"\", "\\"));
            sectionKeys.AddKey("IsPolling", folderConfig.Polling.ToString());
            if (folderConfig.Polling)
            {
                sectionKeys.AddKey("DestinationFolder", folderConfig.MoveToFolder);
                sectionKeys.AddKey("PollingType", folderConfig.PollingType.ToString());
                sectionKeys.AddKey("ApiUrl", folderConfig.ApiUrl);
            }

            sectionData.Keys = sectionKeys;

            return sectionData;
        }

        private IniData GetIniDataOrCreateFile()
        {
            this._logger.LogInformation($"Reading {CONFIG_FILE} ini at: {this._rootDir}");
            var parser = new FileIniDataParser();
            IniData data;

            // create directory
            if (!Directory.Exists(this._rootDir))
            {
                Directory.CreateDirectory(this._rootDir);
            }
            // Read existing ini file or create a default one
            data = !File.Exists(this._iniPath) ? this.CreateDefaultConfig() : parser.ReadFile(this._iniPath);

            // Set default 60 seconds delay if it is missing
            if (string.IsNullOrEmpty(data.Global.GetKeyData("PollingInterval")?.Value))
            {
                data.Global.AddKey("PollingInterval", 60.ToString());
            }
            // Set default instancename if it is missing
            if (string.IsNullOrEmpty(data.Global.GetKeyData("InstanceName")?.Value))
            {
                data.Global.AddKey("InstanceName", "default instance");
            }

            return data;
        }

        private IniData CreateDefaultConfig()
        {
            var parser = new FileIniDataParser();
            IniData data;

            this._logger.LogInformation($"Config file not found, writing new {CONFIG_FILE} ini to: {this._rootDir}");
            data = new IniData();

            // Add default 60seconds delay for polling
            data.Global.AddKey("PollingInterval", 60.ToString());

            // Add default instance name if it is missing
            data.Global.AddKey("InstanceName", "default instance");

            // File does not exist, create it
            parser.WriteFile(this._iniPath, data);

            return data;
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
                this.GetPollingConfigForFolder(folderConfig, section);
            }
            return folderConfig;
        }

        private void GetPollingConfigForFolder(FolderConfig folderConfig, SectionData section)
        {
            var pollingTypeString = section.Keys.GetKeyData("PollingType").Value;
            folderConfig.PollingType = pollingTypeString;

            var destinationFolder = section.Keys.GetKeyData("DestinationFolder").Value;
            folderConfig.MoveToFolder = destinationFolder;

            var folderAPiUrl = section.Keys.GetKeyData("ApiUrl").Value;
            folderConfig.ApiUrl = folderAPiUrl;
        }
    }
}
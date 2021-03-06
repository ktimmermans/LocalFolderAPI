using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.DTO;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSFolderPlugin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FolderController : ControllerBase
    {
        private readonly ILogger<FolderController> _logger;
        private readonly IFolderManagerService _folderManager;
        private readonly IFolderService _folderService;

        public FolderController(
            ILogger<FolderController> logger,
            IFolderManagerService folderManager,
            IFolderService folderService)
        {
            this._logger = logger;
            this._folderManager = folderManager;
            this._folderService = folderService;
        }

        /// <summary>
        /// Get a list of all directories in a folder
        /// </summary>
        /// <returns>A list of directories</returns>
        [HttpGet("{rootDirectory}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<string>> Get(string rootDirectory)
        {
            IEnumerable<string> dirs = new List<string>();
            try
            {
                dirs = this._folderService.GetDirectoriesInFolder(rootDirectory);
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Listing directory for: {rootDirectory} failed with: {ex.Message}");
                return this.BadRequest(new { error = $"Listing directory for: {rootDirectory} failed with: {ex.Message}" });
            }

            return this.Ok(dirs);
        }


        [HttpGet("{folderName}/settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<FolderConfig> GetFolderConfigByFolderName(string folderName)
        {
            var folder = new FolderConfig();
            try
            {
                folder = this._folderManager.GetFolderConfigByFolderName(folderName);
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Getting configuration for: {folderName} failed with: {ex.Message}");
                return this.BadRequest(new { error = $"Getting configuration for: {folderName} failed with: {ex.Message}" });
            }

            return this.Ok(folder);
        }

        /// <summary>
        /// Get a list of all folders currently configured
        /// </summary>
        /// <returns>A list of folders including their settings and properties</returns>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<FolderConfig> Get()
        {
            var configs = this._folderManager.GetAllConfiguredFolders();

            return configs;
        }

        /// <summary>
        /// Get a list of all folders currently configured
        /// </summary>
        /// <returns>A list of folders including their settings and properties</returns>
        [HttpGet("{folderName}/files")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<string> GetFilesFromFolder(string folderName)
        {
            var fileList = this._folderManager.GetAllFilesForFolder(folderName);

            return fileList;
        }

        /// <summary>
        /// Add a folder to the configuration
        /// </summary>
        /// <returns>Nothing</returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post(FolderConfig folderConfig)
        {
            // Add a folder to the configuration
            this._folderManager.AddFolder(folderConfig);

            return this.Ok();
        }


        /// <summary>
        /// Update folder in configuration
        /// </summary>
        /// <returns>Nothing</returns>
        [HttpPost("{folderNameOld}/update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateFolder(FolderConfig folderConfigNew, string folderNameOld)
        {
            // Add new folder to the configuration and delete the old
            this._folderManager.UpdateFolder(folderConfigNew, folderNameOld);

            return this.Ok();
        }
        
        /// <summary>
        /// Delete folder in configuration
        /// </summary>
        /// <returns>Nothing</returns>
        [HttpPost("{folderName}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteFolder(string folderName)
        {
            this._folderManager.DeleteFolder(this._folderManager.GetFolderConfigByFolderName(folderName));

            return this.Ok();
        }

        /// <summary>
        /// Upload a file to the configured folder (if found)
        /// </summary>
        /// <param name="folderName">The folder to upload the file to</param>
        /// <param name="file">The file to upload to the folder</param>
        /// <param name="fileSpecifications">The specifications for the file</param>
        /// <returns>Nothing</returns>
        [HttpPost("{folderName}/file/add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostFile(string folderName, IFormFile file, [FromForm] FileSpec fileSpecifications)
        {

            if (string.IsNullOrEmpty(fileSpecifications.Extension))
            {
                fileSpecifications.Extension = System.IO.Path.GetExtension(file.FileName).Replace(".", "");
            }

            if (string.IsNullOrEmpty(fileSpecifications.FileName))
            {
                fileSpecifications.FileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
            }

            await this._folderManager.AddFileToFolder(folderName, file.OpenReadStream(), fileSpecifications);

            return this.Ok();
        }
    }
}

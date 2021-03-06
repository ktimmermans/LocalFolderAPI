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
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(
            ILogger<TestController> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Get a list of all directories in a folder
        /// </summary>
        /// <returns>A list of directories</returns>
        [HttpPost("receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<FileSpec> Get([FromForm] IFormFile file)
        {
            var request = this.Request;
            var filename = file.FileName;
            var fileSpec = new FileSpec()
            {
                Extension = $".{filename.Split(".").Last()}",
                FileName = filename.Split(".").First(),
            };

            if (string.IsNullOrEmpty(fileSpec.FileName))
            {
                return this.BadRequest("Filename was not specified");
            }

            if (string.IsNullOrEmpty(fileSpec.Extension))
            {
                return this.BadRequest("Extension was not specified");
            }

            return this.Ok(fileSpec);
        }
    }
}

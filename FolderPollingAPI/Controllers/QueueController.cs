using BackgroundWorker.TaskManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using Services.Models;
using System.Collections.Generic;

namespace PSFolderPlugin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IBackgroundTaskManager _taskManager;

        public QueueController(
            ILogger<QueueController> logger,
            IBackgroundTaskManager taskManager)
        {
            this._logger = logger;
            this._taskManager = taskManager;
        }

        /// <summary>
        /// Get a list of all directories in a folder
        /// </summary>
        /// <returns>A list of directories</returns>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<QueuedTask> Get()
        {
            var currentQueues = this._taskManager.GetCurrentBackgroundTasks();

            foreach (var qt in currentQueues)
            {
                string folderName = string.Empty;
                if (qt.GetType() == typeof(PollingTaskDescriptor))
                {
                    folderName = ((PollingTaskDescriptor)qt).FolderName;
                }
                yield return new QueuedTask
                {
                    FolderName = folderName,
                    Name = qt.GetName(),
                    NextRun = qt.GetNextRunTime(),
                };
            }
        }
    }
}

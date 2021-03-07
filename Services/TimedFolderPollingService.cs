using BackgroundWorker.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.FolderConfiguration;
using Services.Interfaces;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class TimedFolderPollingService : ObjectBackgroundWorker<PollingTaskDescriptor>
    {
        private readonly ILogger<TimedFolderPollingService> _logger;
        private readonly IObjectBackgroundQueue<PollingTaskDescriptor> _queue;

        public TimedFolderPollingService(
            IObjectBackgroundQueue<PollingTaskDescriptor> queue,
            IServiceScopeFactory scopeFactory,
            ILogger<TimedFolderPollingService> logger) : base(queue, scopeFactory, logger)
        {
            this._queue = queue;
            this._logger = logger;

            this._logger.LogInformation($"Initiating folderpolling");
            this._queue.Enqueue(new PollingTaskDescriptor
            {
                DelayMilliSeconds = 500,
            });
        }

        /// <summary>
        /// Polling tasks that create an exception will trigger this function in order to handle the exception
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="ex"></param>
        /// <returns>Nothing</returns>
        public override async Task ProcessTaskException(IServiceScope scope, Exception ex)
        {
            this._logger.LogError($"Failed to complete polling folders with exception: {ex.Message}");
        }

        /// <summary>
        /// Pick up a background folder polling task and process it
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="itemToProcess"></param>
        /// <returns>Nothing</returns>
        public override async Task RunTaskWithItem(IServiceScope scope, PollingTaskDescriptor itemToProcess)
        {
            try
            {
                var _folderManagerService = scope.ServiceProvider.GetRequiredService<IFolderManagerService>();
                var allFolders = _folderManagerService.GetAllConfiguredFolders();

                if (string.IsNullOrEmpty(itemToProcess.FolderName))
                {
                    await this.PollAllFolders(scope);
                }
                else
                {
                    var folderToPoll = await _folderManagerService.GetFolderConfigByFolderName(itemToProcess.FolderName);
                    await this.PollFolder(scope, folderToPoll);

                }
            }
            catch (Exception ex)
            {
                await this.ReQueue(scope, itemToProcess.FolderName);
                throw;
            }
        }


        /// <summary>
        /// Process a single folder and create a new background task to process them again
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="folderToPoll"></param>
        /// <returns>Nothing</returns>
        private async Task PollFolder(IServiceScope scope, FolderConfig folderToPoll)
        {
            using (var newScope = scope.ServiceProvider.CreateScope())
            {
                this._logger.LogInformation($"Polling folder: {folderToPoll.FolderName}");
                var _pollingManager = newScope.ServiceProvider.GetRequiredService<IFolderPollingManagerService>();

                var pollingTask = _pollingManager.CreatePollingTaskForFolder(folderToPoll);
                await pollingTask;

                await this.ReQueue(newScope, folderToPoll.FolderName);
            }
        }

        /// <summary>
        /// Fetch a list of all configured folders and poll these. Then create background tasks for all folders to process them again
        /// </summary>
        /// <param name="scope"></param>
        /// <returns>Nothing</returns>
        private async Task PollAllFolders(IServiceScope scope)
        {
            using (var newScope = scope.ServiceProvider.CreateScope())
            {

                _logger.LogDebug("Polling configured folders");
                var _folderManagerService = newScope.ServiceProvider.GetRequiredService<IFolderManagerService>();
                var foldersToPoll = await _folderManagerService.GetAllFoldersToPoll();

                List<Task> pollTaskList = new List<Task>();

                foreach (var folder in foldersToPoll)
                {
                    pollTaskList.Add(PollFolder(newScope, folder));
                }

                if (pollTaskList.Count == 0)
                {
                    await this.ReQueue(newScope, string.Empty);
                }

                this._logger.LogInformation($"Starting {pollTaskList.Count} polling tasks");
                Task.WaitAll(pollTaskList.AsParallel().WithDegreeOfParallelism(10).ToArray());
                this._logger.LogInformation("finished all polling tasks");
            }
        }

        /// <summary>
        /// Create a new background task to process the folder again
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="folderName"></param>
        /// <returns>Nothing</returns>
        private async Task ReQueue(IServiceScope scope, string folderName)
        {
            using (var newScope = scope.ServiceProvider.CreateScope())
            {
                var _folderConfigService = newScope.ServiceProvider.GetRequiredService<IFolderConfigService>();
                var delaySeconds = await _folderConfigService.GetPollingInterval();
                var pollingSettings = new PollingTaskDescriptor
                {
                    DelayMilliSeconds = (delaySeconds * 1000),
                    FolderName = folderName,
                };

                this._logger.LogInformation($"Beginning next poll for folder: {folderName} in: {delaySeconds} seconds");

                this._queue.Enqueue(pollingSettings);
            }
        }
    }
}

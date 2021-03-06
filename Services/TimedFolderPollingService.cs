using Background;
using Background.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class TimedFolderPollingService : VoidBackgroundWorker<PollingTaskDescriptor>
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

        public override async Task ProcessTaskException(IServiceScope scope, Exception ex)
        {
            this._logger.LogError($"Failed to complete polling folders with exception: {ex.Message}");
            this.ReQueue(scope);
        }

        public override async Task RunTask(IServiceScope scope)
        {
            _logger.LogDebug("Polling configured folders");
            var _folderManagerService = scope.ServiceProvider.GetRequiredService<IFolderManagerService>();
            var _pollingManager = scope.ServiceProvider.GetRequiredService<IFolderPollingManagerService>();

            var foldersToPoll = _folderManagerService.GetAllFoldersToPoll();

            List<Task> pollTaskList = new List<Task>();

            foreach (var folder in foldersToPoll)
            {
                var pollingTask = _pollingManager.CreatePollingTaskForFolder(folder);
                pollTaskList.Add(pollingTask);
            }

            this._logger.LogInformation($"Starting {pollTaskList.Count} polling tasks");
            Task.WaitAll(pollTaskList.AsParallel().WithDegreeOfParallelism(10).ToArray());
            this._logger.LogInformation("finished all polling tasks");

            this.ReQueue(scope);
        }

        private void ReQueue(IServiceScope scope)
        {

            var _folderConfigService = scope.ServiceProvider.GetRequiredService<IFolderConfigService>();
            var delaySeconds = _folderConfigService.GetPollingInterval();
            var pollingSettings = new PollingTaskDescriptor
            {
                DelayMilliSeconds = (delaySeconds * 1000),
            };

            this._logger.LogInformation($"Beginning next poll in: {delaySeconds} seconds");

            this._queue.Enqueue(pollingSettings);
        }
    }
}

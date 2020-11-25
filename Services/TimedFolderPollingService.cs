
using Glitter.BackgroundWorker;
using Glitter.BackgroundWorker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class TimedFolderPollingService : TimedTask
    {
        private readonly ILogger<TimedTask> _logger;
        private static int DELAYSECONDS = 60; // 10 seconds

        public TimedFolderPollingService(IServiceProvider serviceProvider,
                                     ILogger<TimedTask> logger) : base(serviceProvider, logger, DELAYSECONDS)
        {
            this._logger = logger;
        }

        public override QueuedTask TaskToRun(CancellationToken stoppingToken)
        {
            var taskToRun = new QueuedTask
            {
                MaxQueuedAmount = 1,
                Name = "FolderPoller",
                Task = PollFolders(),
            };

            return taskToRun;
        }

        private async Task PollFolders()
        {
            _logger.LogDebug("Polling configured folders");
            var _folderManagerService = base.Scope.ServiceProvider.GetRequiredService<IFolderManagerService>();
            var _pollingManager = base.Scope.ServiceProvider.GetRequiredService<IFolderPollingManagerService>();

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
        }
    }
}

namespace WhMgr.HostedServices.TaskQueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Gets a value determining the length of the background task item queue
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Schedules a task which needs to be processed.
        /// </summary>
        /// <param name="workItem">Task item to be executed</param>
        /// <returns></returns>
        ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> workItem);

        /// <summary>
        /// Attempts to remove and return the object at the beginning of the queue.
        /// </summary>
        /// <param name="cancellationToken">Task cancellation token.</param>
        /// <returns></returns>
        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxBatchSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Func<CancellationToken, ValueTask>>> DequeueMultipleAsync(
                    int maxBatchSize,
                    CancellationToken cancellationToken);
    }
}
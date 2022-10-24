namespace WhMgr.HostedServices.TaskQueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;

    using WhMgr.Extensions;

    public class DefaultBackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly int _capacity;
        private Channel<Func<CancellationToken, ValueTask>> _queue;

        public uint Count => Convert.ToUInt32(_queue?.Reader?.Count ?? 0);

        public DefaultBackgroundTaskQueue(int capacity = 4096)
        {
            _capacity = capacity;
            _queue = CreateQueue(_capacity);
        }

        public async ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }

        public async Task<List<Func<CancellationToken, ValueTask>>> DequeueMultipleAsync(
            int maxBatchSize,
            CancellationToken cancellationToken)
        {
            var workItems = await _queue.Reader.ReadMultipleAsync(maxBatchSize, cancellationToken);
            return workItems;
        }

        public void ClearQueue()
        {
            // Clear queue items
            _queue = CreateQueue(_capacity);
        }

        private static Channel<Func<CancellationToken, ValueTask>> CreateQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                Capacity = capacity,
            };
            var queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
            return queue;
        }
    }
}
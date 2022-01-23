namespace WhMgr.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Channels;
	using System.Threading.Tasks;

	public static class ChannelReaderExtensions
	{
		public static async Task<List<T>> ReadMultipleAsync<T>(this ChannelReader<T> reader, int maxBatchSize, CancellationToken cancellationToken)
		{
			await reader.WaitToReadAsync(cancellationToken);

			var batch = new List<T>();
			while (batch.Count < maxBatchSize && reader.TryRead(out T message))
			{
				batch.Add(message);
			}
			return batch;
		}
	}
}
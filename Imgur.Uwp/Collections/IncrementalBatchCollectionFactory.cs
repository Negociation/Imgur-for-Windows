using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Imgur.Collections;
namespace Imgur.Uwp.Collections
{
    public class IncrementalBatchCollectionFactory : IIncrementalCollectionFactory
    {
        public IIncrementalCollection<T> Create<T>(
            Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
            int pageSize = 60, int batchSize = 10, bool autoTriggerNextPage = true)
            => new IncrementalBatchCollection<T>(fetchPage, pageSize, batchSize, autoTriggerNextPage );
    }
}
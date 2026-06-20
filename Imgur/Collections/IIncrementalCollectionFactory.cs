using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.Collections
{
    public interface IIncrementalCollectionFactory
    {
        IIncrementalCollection<T> Create<T>(Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage, int pageSize = 60, int batchSize = 10);
    }
}


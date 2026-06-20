using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.Collections
{
    public interface IIncrementalCollection<T> : IList<T>, INotifyCollectionChanged
    {
        bool CanLoadMorePages { get; }
        bool IsLoading { get; }
        Task LoadNextPageAsync(CancellationToken ct = default(CancellationToken));
        event EventHandler StateChanged;
    }
}

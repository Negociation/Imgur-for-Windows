using Imgur.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Imgur.Uwp.Collections
{
    public class IncrementalBatchCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading, IIncrementalCollection<T>
    {
        private readonly Func<int, CancellationToken, Task<IReadOnlyList<T>>> _fetchPage;
        private readonly int _pageSize;
        private readonly int _batchSize;
        private readonly bool _autoTriggerNextPage;

        private readonly List<T> _buffer = new List<T>();
        private int _bufferIndex;
        private int _nextPage;
        private bool _hasMoreApiPages = true;
        private bool _isLoading;

        public IncrementalBatchCollection(
            Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
            int pageSize = 60,
            int batchSize = 10,
            bool autoTriggerNextPage = true)
        {

            _fetchPage = fetchPage;
            _pageSize = pageSize;
            _batchSize = batchSize;
            _autoTriggerNextPage = autoTriggerNextPage;
            Debug.WriteLine($"[Batch] Criado com autoTrigger={_autoTriggerNextPage}");

        }

        // ── ISupportIncrementalLoading ────────────────────────────────────

        // Manual: para quando buffer vazio (botão decide quando buscar mais)
        // Auto:   mantém grid vivo enquanto houver páginas na API
        public bool HasMoreItems => _autoTriggerNextPage
            ? (_bufferIndex < _buffer.Count || _hasMoreApiPages)
            : (_bufferIndex < _buffer.Count);

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            => AsyncInfo.Run(_ => Task.FromResult(DeliverBatch()));

        // ── IIncrementalCollection ────────────────────────────────────────

        // true quando buffer esgotou E ainda há páginas na API
        // usado pela VM do botão para mostrar/esconder o LoadMore
        public bool CanLoadMorePages => _hasMoreApiPages && _bufferIndex >= _buffer.Count;

        public bool IsLoading => _isLoading;

        public event EventHandler StateChanged;

        // ── Lógica interna ────────────────────────────────────────────────

        private LoadMoreItemsResult DeliverBatch()
        {
            if (_bufferIndex >= _buffer.Count)
                return new LoadMoreItemsResult { Count = 0 };

            uint delivered = 0;
            int end = Math.Min(_bufferIndex + _batchSize, _buffer.Count);
            for (int i = _bufferIndex; i < end; i++) { Add(_buffer[i]); delivered++; }
            _bufferIndex = end;
            Debug.WriteLine($"[Batch] Entregou {delivered} items (bufferIndex={_bufferIndex}/{_buffer.Count})");

            // Buffer esgotou neste batch
            if (_bufferIndex >= _buffer.Count)
            {
                if (_autoTriggerNextPage && _hasMoreApiPages && !_isLoading)
                {
                    _ = LoadNextPageAsync();
                    Debug.WriteLine("[Batch] Buffer vazio → auto load");
                }
                else if (!_autoTriggerNextPage && _hasMoreApiPages)
                {
                    StateChanged?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine("[Batch] Buffer vazio → botão load more");
                }
            }

            return new LoadMoreItemsResult { Count = delivered };
        }

        // Chamado pelo botão (manual) ou pela VM ao receber StateChanged (auto)
        public async Task LoadNextPageAsync(CancellationToken ct = default(CancellationToken))
        {
            Debug.WriteLine($"[Batch] LoadNextPageAsync chamado — stack: {Environment.StackTrace}");

            if (_isLoading || !_hasMoreApiPages) return;
            _isLoading = true;
            try
            {
                Debug.WriteLine($"[Batch] Buscando página {_nextPage} na API...");
                var page = await _fetchPage(_nextPage, ct);

                if (page == null || page.Count == 0)
                {
                    _hasMoreApiPages = false;
                    Debug.WriteLine("[Batch] API sem mais dados");
                    return;
                }

                _buffer.Clear();
                _buffer.AddRange(page);
                _bufferIndex = 0;

                if (page.Count < _pageSize)
                {
                    _hasMoreApiPages = false;
                    Debug.WriteLine("[Batch] Última página recebida");
                }
                else
                {
                    _nextPage++;
                }

                DeliverBatch(); // entrega 1º batch; resto vem via scroll
            }
            finally
            {
                _isLoading = false;
                // Notifica sempre: atualiza CanLoadMorePages, HasFavorites, etc.
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
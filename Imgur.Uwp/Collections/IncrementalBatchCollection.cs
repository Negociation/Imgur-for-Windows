using Imgur.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Imgur.Uwp.Collections
{
    public class IncrementalBatchCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading, IIncrementalCollection<T>
    {
        private readonly Func<int, CancellationToken, Task<IReadOnlyList<T>>> _fetchPage; // a "LoadMore de fora"
        private readonly int _pageSize;   // 60
        private readonly int _batchSize;  // 10
        private readonly List<T> _buffer = new List<T>();
        private int _bufferIndex;
        private int _nextPage;
        private bool _hasMoreApiPages = true;
        private bool _isLoading;
        public IncrementalBatchCollection(
            Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
            int pageSize = 60, int batchSize = 10)
        {
            _fetchPage = fetchPage;
            _pageSize = pageSize;
            _batchSize = batchSize;
        }
        // AUTO: true só enquanto sobra item NO BUFFER. Buffer vazio => grid para.
        public bool HasMoreItems => _bufferIndex < _buffer.Count;
        // Para o BOTÃO: terminou o bloco atual E ainda existe próxima página na API.
        public bool CanLoadMorePages => _hasMoreApiPages && _bufferIndex >= _buffer.Count;

        // Esta carregando
        public bool IsLoading => _isLoading;

        // Chamado automaticamente pelo AdaptiveGridView (Edge): só pinga do buffer.
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            => AsyncInfo.Run(_ => Task.FromResult(DeliverBatch()));


        private LoadMoreItemsResult DeliverBatch()
        {
            uint delivered = 0;
            int end = Math.Min(_bufferIndex + _batchSize, _buffer.Count);
            for (int i = _bufferIndex; i < end; i++) { Add(_buffer[i]); delivered++; }
            _bufferIndex = end;
            Debug.WriteLine("Chamando batch");
            if (_bufferIndex >= _buffer.Count)
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("Finalizei Pagina inteira");
            }

            return new LoadMoreItemsResult { Count = delivered };
        }

        // MANUAL: chamado pelo COMMAND do botão. Busca +60 e religa o auto-drip.
        public async Task LoadNextPageAsync(CancellationToken ct = default(CancellationToken))
        {
            if (_isLoading || !_hasMoreApiPages) return;
            _isLoading = true;
            try
            {
                var page = await _fetchPage(_nextPage, ct);
                if (page == null || page.Count == 0) { _hasMoreApiPages = false; return; }
                _buffer.Clear();
                _buffer.AddRange(page);
                _bufferIndex = 0;
                if (page.Count < _pageSize) _hasMoreApiPages = false;
                else _nextPage++;
                DeliverBatch(); // mostra o 1º pedaço; o resto vem via auto-drip ao rolar
            }
            finally
            {
                _isLoading = false;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler StateChanged; // VM escuta p/ atualizar visibilidade do botão
    }
}

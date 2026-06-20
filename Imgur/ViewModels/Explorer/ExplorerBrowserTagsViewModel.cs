using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Services;
using Imgur.ViewModels.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Imgur.ViewModels.Explorer
{
    public class ExplorerBrowserTagsViewModel : ExplorerBrowserViewModelBase
    {
        //***************************************************************
        // View Parameters
        //***************************************************************

        //-- Coleção incremental de Tags (full — sem Take(30))
        public IIncrementalCollection<TagViewModel> TagsVmCollection { get; private set; }

        //-- There's Tags Available to Show
        public bool TagsAvailableToShow => (TagsVmCollection?.Count ?? 0) > 0;

        //***************************************************************
        // Services
        //***************************************************************

        private readonly TagsService _tagsService;
        private readonly ITagVmFactory _tagVmFactory;
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //***************************************************************
        // Constructor
        //***************************************************************

        /// <param name="preloadedPage0">
        /// Lista já carregada (Página 0) vinda do carrossel resumido da ExplorerSearchViewModel.
        /// Será inserida diretamente na coleção, evitando uma chamada de rede redundante.
        /// </param>
        public ExplorerBrowserTagsViewModel(
            IDispatcher dispatcher,
            INavigator navigator,
            TagsService tagsService,
            ITagVmFactory tagVmFactory,
            IIncrementalCollectionFactory collectionFactory,
            string query,
            List<TagViewModel> preloadedPage0)
            : base(query, dispatcher, navigator)
        {
            _tagsService = tagsService;
            _tagVmFactory = tagVmFactory;
            _collectionFactory = collectionFactory;

            BuildCollection(preloadedPage0);
        }

        //***************************************************************
        // Collection Setup
        //***************************************************************

        private void BuildCollection(List<TagViewModel> preloadedPage0)
        {
            TagsVmCollection = _collectionFactory.Create<TagViewModel>(
                async (page, ct) =>
                {
                    // Página 0 já foi pré-carregada: devolve diretamente sem chamada de rede
                    if (page == 0)
                        return preloadedPage0 ?? new List<TagViewModel>();

                    var result = await _tagsService.SearchTags(Query, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<TagViewModel>();

                    return result.Data
                        .Where(t => t != null)
                        .Select(_tagVmFactory.GetTagViewModel)
                        .ToList();
                },
                pageSize: 60,
                batchSize: 12);

            TagsVmCollection.StateChanged += OnCollectionStateChanged;
            OnPropertyChanged(nameof(TagsVmCollection));
        }

        //***************************************************************
        // Initialization
        //***************************************************************

        public async Task InitializeAsync()
        {
            try
            {
                Loading = true;

                await TagsVmCollection.LoadNextPageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Loading = false;
                OnPropertyChanged(nameof(TagsAvailableToShow));
            }
        }

        private void OnCollectionStateChanged(object sender, EventArgs e)
        {
            Dispatcher.CheckBeginInvokeOnUi(() =>
            {
                OnPropertyChanged(nameof(TagsAvailableToShow));
            });
        }
    }
}
using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Services;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.ViewModels.Explorer
{
    public class ExplorerBrowserGalleriesViewModel : ExplorerBrowserViewModelBase
    {
        //***************************************************************
        // View Parameters
        //***************************************************************

        //-- Coleção incremental de Galerias (full — sem Take(30))
        public IIncrementalCollection<MediaViewModel> GalleryMediaVmCollection { get; private set; }

        //-- There's Galleries Available to Show
        public bool GalleriesAvailableToShow => (GalleryMediaVmCollection?.Count ?? 0) > 0;

        //***************************************************************
        // Services
        //***************************************************************

        private readonly GalleryService _galleryService;
        private readonly IMediaVmFactory _mediaVmFactory;
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //***************************************************************
        // Constructor
        //***************************************************************

        /// <param name="preloadedPage0">
        /// Lista já carregada (Página 0) vinda do carrossel resumido da ExplorerSearchViewModel.
        /// Será inserida diretamente na coleção, evitando uma chamada de rede redundante.
        /// </param>
        public ExplorerBrowserGalleriesViewModel(
            IDispatcher dispatcher,
            INavigator navigator,
            GalleryService galleryService,
            IMediaVmFactory mediaVmFactory,
            IIncrementalCollectionFactory collectionFactory,
            string query,
            List<MediaViewModel> preloadedPage0)
            : base(query, dispatcher, navigator)
        {
            _galleryService = galleryService;
            _mediaVmFactory = mediaVmFactory;
            _collectionFactory = collectionFactory;

            BuildCollection(preloadedPage0);
        }

        //***************************************************************
        // Collection Setup
        //***************************************************************

        private void BuildCollection(List<MediaViewModel> preloadedPage0)
        {
            GalleryMediaVmCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    Debug.WriteLine("page:"+page);
                    // Página 0 já foi pré-carregada: devolve diretamente sem chamada de rede
                    if (page == 0)
                        return preloadedPage0 ?? new List<MediaViewModel>();
                    Debug.WriteLine("Carregando nova pagina");
                    var result = await _galleryService.SearchGalleries(Query, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    return result.Data.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: 60,
                batchSize: 12);

            GalleryMediaVmCollection.StateChanged += OnCollectionStateChanged;
            OnPropertyChanged(nameof(GalleryMediaVmCollection));
        }

        //***************************************************************
        // Initialization
        //***************************************************************

        public async Task InitializeAsync()
        {
            try
            {
                Loading = true;

                // Carrega a página 0 (pre-loaded) + dispara o drip inicial
                await GalleryMediaVmCollection.LoadNextPageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Loading = false;
                OnPropertyChanged(nameof(GalleriesAvailableToShow));
            }
        }

        private void OnCollectionStateChanged(object sender, EventArgs e)
        {
            Dispatcher.CheckBeginInvokeOnUi(() =>
            {
                try
                {
                    OnPropertyChanged(nameof(GalleriesAvailableToShow));
                } catch (Exception ex) {
                    Debug.WriteLine("Erro LoadMore: " + ex.Message);
                }
                finally
                {
                    Loading = false;
                }

            });
        }
    }
}
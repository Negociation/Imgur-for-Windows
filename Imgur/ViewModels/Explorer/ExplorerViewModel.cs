using Imgur.Api.Services.Models.Enum;
using Imgur.Collections;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.Services;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Explorer
{
    public class ExplorerViewModel : Observable
    {
        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- Loading Content Flag Property
        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged("Loading");
            }
        }


        //-- Loading Success State Flag Property
        private bool _loadedSuccessfully;

        public bool LoadedSuccessfully
        {
            get { return _loadedSuccessfully; }
            set
            {
                _loadedSuccessfully = value;
                OnPropertyChanged("LoadedSuccessfully");
            }
        }


        //-- Static Sections Avaliable for Selection
        public ObservableCollection<Section> Sections { get; } = new ObservableCollection<Section>();

        //-- Dynamic Sorts Avaliable for Selection (Based on Sections)
        public ObservableCollection<Sort> AvailableSorts { get; } = new ObservableCollection<Sort>();


        //-- Selected Section Index for Explore Gallery
        private int _selectedSectionIndex;

        public int SelectedSectionIndex
        {
            get { return _selectedSectionIndex; }
            set
            {
                _selectedSectionIndex = value;
                OnPropertyChanged("SelectedSectionIndex");

                this.UpdateAvaliableSorts();
            }
        }


        //-- Selected Sort Index for Explore Gallery
        private int _selectedSortIndex;

        public int SelectedSortIndex
        {
            get { return _selectedSortIndex; }
            set
            {
                _selectedSortIndex = value;
                OnPropertyChanged("SelectedSortIndex");
            }
        }

        //-- Thumbnail Config Size
        public int ThumbSize => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);


        //-- Listagem de Itens (MediaViewModel já criados). É o ItemsSource do AdaptiveGridView.
        //   Tipada pela interface NEUTRA -> a VM não conhece nada de UWP.
        public IIncrementalCollection<MediaViewModel> RetrievedMediaVmCollection { get; private set; }


        //-- Indica se ainda há uma próxima página de 60 para o botão "Load More".
        //   Atualizada via evento StateChanged da coleção.
        private bool _canLoadMore;

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set
            {
                _canLoadMore = value;
                OnPropertyChanged("CanLoadMore");
                OnPropertyChanged("ShowLoadMoreButton");
            }
        }


        //-- Visibilidade efetiva do botão: só aparece se há mais páginas E não está carregando.
        public bool ShowLoadMoreButton => CanLoadMore && !IsLoadingNewPage;


        //-- Scroll to Top Flag
        private bool _canScrollToTop;

        public bool CanScrollToTop
        {
            get { return _canScrollToTop; }
            set
            {
                _canScrollToTop = value;
                OnPropertyChanged("CanScrollToTop");
            }
        }


        //-- Status for Loading a new Page (botão / ProgressBar do footer)
        private bool _isLoadingNewPage;

        public bool IsLoadingNewPage
        {
            get { return _isLoadingNewPage; }
            set
            {
                _isLoadingNewPage = value;
                OnPropertyChanged("IsLoadingNewPage");
                OnPropertyChanged("ShowLoadMoreButton");
            }
        }


        //-- Cancellation Token para a busca atual (cancela carga anterior ao trocar seção/sort)
        private CancellationTokenSource _cts = new CancellationTokenSource();


        //***************************************************************
        // Services 
        //***************************************************************

        //-- Service para busca na Galeria
        private readonly GalleryService _galleryService;

        //-- Dispatcher para UI
        private readonly IDispatcher _dispatcher;

        //-- Factory para ViewModel de Media
        private readonly IMediaVmFactory _mediaVmFactory;

        //-- Servico de Settings do App
        private readonly ILocalSettings _localSettings;

        //-- Serviço de Navegação
        private readonly INavigator _navigator;

        //-- Factory da coleção incremental (impl. concreta vive no Imgur.Uwp)
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************
        public ExplorerViewModel(
            GalleryService galleryService,
            IDispatcher dispatcher,
            IMediaVmFactory mediaVmFactory,
            ILocalSettings localSettings,
            INavigator navigator,
            IIncrementalCollectionFactory collectionFactory
            )
        {
            _galleryService = galleryService;
            _dispatcher = dispatcher;
            _mediaVmFactory = mediaVmFactory;
            _localSettings = localSettings;
            _navigator = navigator;
            _collectionFactory = collectionFactory;
            Loading = false;
        }

        //-- View Model Initilization
        public async Task InitializeAsync()
        {
            //If already loaded just ignore 
            if (LoadedSuccessfully) return;

            try
            {
                LoadedSuccessfully = true;

                if (Sections.Count == 0 || AvailableSorts.Count == 0)
                {
                    IReadOnlyList<Section> sections = GalleryConstants.GetSections();

                    foreach (var section in sections)
                    {
                        this.Sections.Add(section);
                    }

                    //Fix for C.U
                    SelectedSortIndex = SelectedSectionIndex = 0;
                }

                if (RetrieveGalleryContentCommand.CanExecute(null)) { RetrieveGalleryContentCommand.Execute(null); }
            }
            catch
            {
                LoadedSuccessfully = false;
                Loading = false;
            }
        }


        //***************************************************************
        // View Commands 
        //***************************************************************

        //-- Command para (re)montar a galeria: troca de seção/sort, refresh e reload.
        private ICommand _retrieveGalleryContentCommand;

        public ICommand RetrieveGalleryContentCommand
        {
            get
            {
                if (_retrieveGalleryContentCommand == null)
                {
                    _retrieveGalleryContentCommand = new RelayCommand(async () =>
                    {
                        await ReloadAsync();
                    });
                }
                return _retrieveGalleryContentCommand;
            }
        }

        //-- Command do botão "Load More": pede a PRÓXIMA página de 60 (manual).
        private ICommand _loadMoreGalleryContentCommand;

        public ICommand LoadMoreGalleryContentCommand
        {
            get
            {
                if (_loadMoreGalleryContentCommand == null)
                {
                    _loadMoreGalleryContentCommand = new RelayCommand(async () =>
                    {
                        if (RetrievedMediaVmCollection == null || IsLoadingNewPage) return;

                        try
                        {
                            // UI: inicia loading do footer
                            IsLoadingNewPage = true;

                            // Busca +60 na API, reabastece o buffer e entrega o 1º batch.
                            // O restante dos 60 é entregue automaticamente pelo grid ao rolar.
                            await RetrievedMediaVmCollection.LoadNextPageAsync(_cts.Token);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Erro LoadMore: " + ex.Message);
                        }
                        finally
                        {
                            IsLoadingNewPage = false;
                        }
                    });
                }

                return _loadMoreGalleryContentCommand;
            }
        }


        //***************************************************************
        // View Functions 
        //***************************************************************

        //-- (Re)constrói a coleção incremental e carrega a primeira página de 60.
        private async Task ReloadAsync()
        {
            try
            {
                // Inicia Loading imediatamente
                Loading = true;
                LoadedSuccessfully = true;

                // Cancela qualquer carga anterior em andamento
                _cts.Cancel();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                // Delay para dar tempo das sections/sorts atualizarem na UI
                await Task.Delay(1000);

                // Captura seção/sort atuais (evita corrida se o usuário trocar depois)
                var section = Sections[SelectedSectionIndex].section;
                var sort = AvailableSorts[SelectedSortIndex].sort;

                // Desinscreve a coleção anterior, se houver
                if (RetrievedMediaVmCollection != null)
                    RetrievedMediaVmCollection.StateChanged -= OnCollectionStateChanged;

                // Monta a coleção incremental:
                //   pageSize 60  -> tamanho da página da API (buffer interno)
                //   batchSize 10 -> quantos itens o grid recebe por "drip" ao rolar
                RetrievedMediaVmCollection = _collectionFactory.Create<MediaViewModel>(
                    async (page, token) =>
                    {
                        var result = await _galleryService.GetExplorerMedia(section, sort, page);

                        if (!result.IsSuccess || result.Data == null)
                            return new List<MediaViewModel>();

                        // Cria PREVIAMENTE os MediaViewModel desta página de 60
                        return result.Data
                                     .Select(_mediaVmFactory.GetMediaViewModel)
                                     .ToList();
                    },
                    60,
                    10,
                    false
                    );

                // Escuta mudanças de estado (fim de bloco / fim de carga) p/ ligar o botão
                RetrievedMediaVmCollection.StateChanged += OnCollectionStateChanged;

                // Notifica o binding do ItemsSource do AdaptiveGridView
                OnPropertyChanged("RetrievedMediaVmCollection");
                CanLoadMore = false;

                // Carrega a primeira página (60) -> entrega o primeiro batch ao grid
                await RetrievedMediaVmCollection.LoadNextPageAsync(ct);

                LoadedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LoadedSuccessfully = false;
            }
            finally
            {
                Loading = false;
            }
        }

        //-- Atualiza a visibilidade do botão "Load More" sempre que a coleção muda de estado.
        private void OnCollectionStateChanged(object sender, EventArgs e)
        {
            _dispatcher.CheckBeginInvokeOnUi(() =>
            {
                CanLoadMore = RetrievedMediaVmCollection != null
                              && RetrievedMediaVmCollection.CanLoadMorePages;
            });
        }

        //-- Update Sorts based on current Selected Section
        private void UpdateAvaliableSorts()
        {

            this.AvailableSorts.Clear();
            foreach (var sort in this.Sections[SelectedSectionIndex].sorts)
            {
                this.AvailableSorts.Add(sort);
            }

            this.SelectedSortIndex = 0;
        }
    }
}
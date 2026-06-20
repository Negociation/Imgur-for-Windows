using Imgur.Collections;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Services;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Media;
using Imgur.ViewModels.Tags;
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
    public class ExplorerSearchViewModel : Observable
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


        //-- Staff Picks ViewModel
        private TagViewModel _staffPicks;

        public TagViewModel StaffPicks
        {
            get { return _staffPicks; }
            set
            {
                _staffPicks = value;
                OnPropertyChanged("StaffPicks");
                OnPropertyChanged("LoadedStaffPicks");
                OnPropertyChanged("StaffPicksItems");
                OnPropertyChanged("LoadedSuccessfully");
            }
        }

        //-- Loaded (Staff Picks Section) - Arrow Function
        public bool LoadedStaffPicks => _staffPicks != null;

        //-- Staff Picks Items
        public IEnumerable<MediaViewModel> StaffPicksItems => StaffPicks?.RetrievedMediaVmCollection?.Take(30);

        //-- Loaded (Search Posts) Content Flag Property
        private bool _loadedSearchPosts;

        public bool LoadedSearchPosts
        {
            get { return _loadedSearchPosts; }
            set
            {
                _loadedSearchPosts = value;
                OnPropertyChanged("LoadedSearchPosts");
            }
        }

        //-- Search Query
        private string _searchQuery;

        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                OnPropertyChanged("SearchQuery");
                OnPropertyChanged(nameof(IsSearchMode));
                OnPropertyChanged(nameof(IsExplorerMode));
            }
        }

        //-- Is Search Mode ?
        public bool IsSearchMode => !string.IsNullOrWhiteSpace(_searchQuery);

        //-- Is Explorer Mode ?
        public bool IsExplorerMode => string.IsNullOrWhiteSpace(_searchQuery);

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

        //-- Listagem de Itens Random (ViewModels) — Explorer mode (grid vertical, sem incremental)
        public ObservableCollection<MediaViewModel> RandomMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

        //-- Carrossel Search "Tags" (incremental / preview)
        public IIncrementalCollection<TagViewModel> TagsSearchPartialMediaVmCollection { get; private set; }

        //-- Complete Search "Users" (Page 0)
        private List<TagViewModel> _tagsSearchMediaVmCollection { get; set; }

        //-- Carrossel Search "Gallery" (incremental / preview)
        public IIncrementalCollection<MediaViewModel> GallerySearchPartialMediaVmCollection { get; private set; }

        //-- Complete Search "Gallery" (Page 0)
        private List<MediaViewModel> _gallerySearchMediaVmCollection { get; set; }

        //-- Carrossel Search "Users" (incremental / preview)
        public IIncrementalCollection<AccountViewModel> UsersSearchPartialMediaVmCollection { get; private set; }

        //-- Complete Search "Users" (Page 0)
        private List<AccountViewModel> _usersSearchMediaVmCollection { get; set; }

        //-- Thumbnail Config Size
        public int ThumbSize => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);

        //-- LoadedSuccessfully state
        public bool LoadedSuccessfully => LoadedStaffPicks;

        //-- Nothing found during search state
        public bool NothingFound => !UsersAvaliableToShow && !GalleriesAvaliableToShow && !TagsAvaliableToShow;

        //-- There's Tags Avaliable to Show during Search
        public bool TagsAvaliableToShow => (TagsSearchPartialMediaVmCollection?.Count ?? 0) > 0;

        //-- There's Galleries Avaliable to Show during Search
        public bool GalleriesAvaliableToShow => (GallerySearchPartialMediaVmCollection?.Count ?? 0) > 0;

        //-- There's Users Avaliable to Show during Search
        public bool UsersAvaliableToShow => (UsersSearchPartialMediaVmCollection?.Count ?? 0) > 0;

        //***************************************************************
        // Services
        //***************************************************************
        //-- Dispatcher para UI
        private readonly IDispatcher _dispatcher;

        //-- Serviço de Navegação
        private readonly INavigator _navigator;

        //Serviço Action de Consulta de Staff Picks para Explorer Mode
        private readonly TagsService _tagsService;

        //-- Factory para criar Tag ViewModels
        private readonly ITagVmFactory _tagVmFactory;

        //Serviço Action de Consulta de Galeria Random para Explorer Mode
        private readonly GalleryService _galleryService;

        //-- Factory para criar Media ViewModels
        private readonly IMediaVmFactory _mediaVmFactory;

        //-- Servico de Settings do App
        private readonly ILocalSettings _localSettings;

        //Serviço Action de Consulta de Usuarios para Search Mode
        private readonly AccountService _accountService;

        //-- Factory para criar Account ViewModels
        private readonly IAccountVmFactory _accountVmFactory;

        //-- Factory da coleção incremental (impl. concreta vive no Imgur.Uwp)
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //-- Factories para as views de Browse (SEE ALL)
        private readonly IExplorerBrowserTagsVmFactory _browserTagsVmFactory;
        private readonly IExplorerBrowserGalleriesVmFactory _browserGalleriesVmFactory;
        private readonly IExplorerBrowserUsersVmFactory _browserUsersVmFactory;

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************
        public ExplorerSearchViewModel(
            IDispatcher dispatcher,
            INavigator navigator,
            TagsService tagsService,
            ITagVmFactory tagVmFactory,
            GalleryService galleryService,
            IMediaVmFactory mediaVmFactory,
            ILocalSettings localSettings,
            AccountService accountService,
            IAccountVmFactory accountVmFactory,
            IIncrementalCollectionFactory collectionFactory,
            IExplorerBrowserTagsVmFactory browserTagsVmFactory,
            IExplorerBrowserGalleriesVmFactory browserGalleriesVmFactory,
            IExplorerBrowserUsersVmFactory browserUsersVmFactory
            )
        {
            _dispatcher = dispatcher;
            _navigator = navigator;
            _tagsService = tagsService;
            _tagVmFactory = tagVmFactory;
            _galleryService = galleryService;
            _mediaVmFactory = mediaVmFactory;
            _localSettings = localSettings;
            _accountService = accountService;
            _accountVmFactory = accountVmFactory;
            _collectionFactory = collectionFactory;
            _browserTagsVmFactory = browserTagsVmFactory;
            _browserGalleriesVmFactory = browserGalleriesVmFactory;
            _browserUsersVmFactory = browserUsersVmFactory;
        }

        public void InitializeSearch(string query)
        {
            this.Initialize();

            _searchQuery = query;
        }

        public void Initialize() { }

        public async Task InitializeAsync()
        {
            //If already loaded just ignore 
            if (LoadedSuccessfully)
            {
                return;
            }

            Loading = true;

            if (IsExplorerMode)
            {
                if (RetrieveExplorerDataCommand.CanExecute(null)) { RetrieveExplorerDataCommand.Execute(null); }
                return;
            }

            if (IsSearchMode)
            {
                if (RetrieveSearchDataCommand.CanExecute(_searchQuery)) { RetrieveSearchDataCommand.Execute(_searchQuery); }
                return;
            }
        }


        //***************************************************************
        // View Commands
        //***************************************************************

        //-- Command de Back Button
        private ICommand _leaveCurrentPage;

        public ICommand LeaveCurrentPage
        {
            get
            {
                if (_leaveCurrentPage == null)
                {
                    _leaveCurrentPage = new RelayCommand(() =>
                    {
                        _navigator.GoBack();
                    });
                }
                return _leaveCurrentPage;
            }
        }

        // Command para Refresh da View Atual
        private ICommand _reloadCurrentPage;

        public ICommand ReloadCurrentPage
        {
            get
            {
                if (_reloadCurrentPage == null)
                {
                    _reloadCurrentPage = new RelayCommand(async () =>
                    {
                        await InitializeAsync();
                    });
                }
                return _reloadCurrentPage;
            }
        }


        //-- Command para Carregar Dados do Explorer (Staff Picks + Random)
        private ICommand _retrieveExplorerDataCommand;

        public ICommand RetrieveExplorerDataCommand
        {
            get
            {
                if (_retrieveExplorerDataCommand == null)
                {
                    _retrieveExplorerDataCommand = new RelayCommand(() =>
                    {
                        // Inicia Loading imediatamente
                        Loading = true;

                        // Explorer mode usa ObservableCollection comum -> pode ficar em background
                        Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(10);
                                await RetrieveStaffPicksAsync();
                                await RetrieveRandomMediaAsync();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            finally
                            {
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    Loading = false;
                                });
                            }
                        });
                    });
                }
                return _retrieveExplorerDataCommand;
            }
        }


        private async Task RetrieveStaffPicksAsync()
        {
            try
            {
                var retrivedTagContent = await _tagsService.GetStaffPicks();

                if (!retrivedTagContent.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrivedTagContent.Error);
                }

                var vm = _tagVmFactory.GetTagViewModel(retrivedTagContent.Data);

                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    StaffPicks = vm;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task RetrieveRandomMediaAsync()
        {
            try
            {
                var retrievedRandomMedia = await _galleryService.GetRandomMedia();

                if (!retrievedRandomMedia.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrievedRandomMedia.Error);
                }

                var vmList = new List<MediaViewModel>();
                foreach (var media in retrievedRandomMedia.Data)
                {
                    vmList.Add(_mediaVmFactory.GetMediaViewModel(media));
                }

                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    RandomMediaVmCollection.Clear();
                    foreach (var vm in vmList.Take(20))
                        RandomMediaVmCollection.Add(vm);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //-- Command para Carregar Dados de Search (carrosséis incrementais)
        private ICommand _retrieveSearchDataCommand;

        public ICommand RetrieveSearchDataCommand
        {
            get
            {
                if (_retrieveSearchDataCommand == null)
                {
                    // async na UI thread: a IncrementalBatchCollection faz o Add dela mesma,
                    // então NÃO pode rodar em Task.Run.
                    _retrieveSearchDataCommand = new RelayCommand<string>(async (query) =>
                    {
                        try
                        {
                            Loading = true;

                            await BuildGalleriesCarouselAsync(query);
                            await BuildTagsCarouselAsync(query);
                            await BuildUsersCarouselAsync(query);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        finally
                        {
                            Loading = false;
                            OnPropertyChanged(nameof(NothingFound));
                        }
                    });
                }
                return _retrieveSearchDataCommand;
            }
        }

        //-- Command para explorar Usuários Encontados
        private ICommand _browseUsersCommand;

        public ICommand BrowseUsersCommand
        {
            get
            {
                if (_browseUsersCommand == null)
                {
                    _browseUsersCommand = new RelayCommand(() =>
                    {
                        var vm = _browserUsersVmFactory.GetViewModel(
                            _searchQuery,
                            _usersSearchMediaVmCollection ?? new List<AccountViewModel>());

                        _navigator.Navigate("explorerBrowserUsers", vm);
                    });
                }
                return _browseUsersCommand;
            }
        }

        //-- Command para explorar Tags Encontados
        private ICommand _browseTagsCommand;

        public ICommand BrowseTagsCommand
        {
            get
            {
                if (_browseTagsCommand == null)
                {
                    _browseTagsCommand = new RelayCommand(() =>
                    {
                        var vm = _browserTagsVmFactory.GetViewModel(
                            _searchQuery,
                            _tagsSearchMediaVmCollection ?? new List<TagViewModel>());

                        _navigator.Navigate("explorerBrowserTags", vm);
                    });
                }
                return _browseTagsCommand;
            }
        }

        //-- Command para explorar Galerias Encontados
        private ICommand _browseGalleriesCommand;

        public ICommand BrowseGalleriesCommand
        {
            get
            {
                if (_browseGalleriesCommand == null)
                {
                    _browseGalleriesCommand = new RelayCommand(() =>
                    {
                        var vm = _browserGalleriesVmFactory.GetViewModel(
                            _searchQuery,
                            _gallerySearchMediaVmCollection ?? new List<MediaViewModel>());

                        _navigator.Navigate("explorerBrowserGalleries", vm);
                    });
                }
                return _browseGalleriesCommand;
            }
        }

        //***************************************************************
        // Construção dos carrosséis incrementais (preview de 1 página)
        //***************************************************************

        private async Task BuildGalleriesCarouselAsync(string query)
        {
            if (GallerySearchPartialMediaVmCollection != null)
                GallerySearchPartialMediaVmCollection.StateChanged -= OnSearchCollectionStateChanged;

            GallerySearchPartialMediaVmCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    // Preview: só a 1ª página. (Para "infinito" no carrossel, é só deixar paginar.)
                    if (page > 0) return new List<MediaViewModel>();

                    var result = await _galleryService.SearchGalleries(query, 0);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    var resultData = result.Data.Select(_mediaVmFactory.GetMediaViewModel);

                    this._gallerySearchMediaVmCollection = resultData.ToList();
                    return resultData.Take(30).ToList();
                },
                30,   // pageSize (teto do preview)
                8);   // batchSize (drip horizontal)

            GallerySearchPartialMediaVmCollection.StateChanged += OnSearchCollectionStateChanged;
            OnPropertyChanged(nameof(GallerySearchPartialMediaVmCollection));

            await GallerySearchPartialMediaVmCollection.LoadNextPageAsync();

            OnPropertyChanged(nameof(GalleriesAvaliableToShow));
        }

        private async Task BuildTagsCarouselAsync(string query)
        {
            if (TagsSearchPartialMediaVmCollection != null)
                TagsSearchPartialMediaVmCollection.StateChanged -= OnSearchCollectionStateChanged;

            TagsSearchPartialMediaVmCollection = _collectionFactory.Create<TagViewModel>(
                async (page, ct) =>
                {
                    if (page > 0) return new List<TagViewModel>();

                    var result = await _tagsService.SearchTags(query, 0);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<TagViewModel>();

                    var resultData = result.Data
                            .Where(t => t != null)
                            .Select(_tagVmFactory.GetTagViewModel);

                    this._tagsSearchMediaVmCollection = resultData.ToList();

                    return this._tagsSearchMediaVmCollection.Take(30).ToList();
                },
                30,
                8);

            TagsSearchPartialMediaVmCollection.StateChanged += OnSearchCollectionStateChanged;
            OnPropertyChanged(nameof(TagsSearchPartialMediaVmCollection));

            await TagsSearchPartialMediaVmCollection.LoadNextPageAsync();

            OnPropertyChanged(nameof(TagsAvaliableToShow));
        }

        private async Task BuildUsersCarouselAsync(string query)
        {
            if (UsersSearchPartialMediaVmCollection != null)
                UsersSearchPartialMediaVmCollection.StateChanged -= OnSearchCollectionStateChanged;

            UsersSearchPartialMediaVmCollection = _collectionFactory.Create<AccountViewModel>(
                async (page, ct) =>
                {
                    if (page > 0) return new List<AccountViewModel>();

                    var result = await _accountService.SearchAccounts(query, 0);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<AccountViewModel>();

                    var resultData = result.Data.Where(u => u != null).Select(_accountVmFactory.GetAccountViewModel);
                    this._usersSearchMediaVmCollection = resultData.ToList();
                    return resultData.Take(30).ToList();
                },
                30,
                8);

            UsersSearchPartialMediaVmCollection.StateChanged += OnSearchCollectionStateChanged;
            OnPropertyChanged(nameof(UsersSearchPartialMediaVmCollection));

            await UsersSearchPartialMediaVmCollection.LoadNextPageAsync();

            OnPropertyChanged(nameof(UsersAvaliableToShow));
        }

        //-- Atualiza visibilidade das seções conforme os carrosséis drenam.
        private void OnSearchCollectionStateChanged(object sender, EventArgs e)
        {
            _dispatcher.CheckBeginInvokeOnUi(() =>
            {
                OnPropertyChanged(nameof(TagsAvaliableToShow));
                OnPropertyChanged(nameof(GalleriesAvaliableToShow));
                OnPropertyChanged(nameof(UsersAvaliableToShow));
                OnPropertyChanged(nameof(NothingFound));
            });
        }
    }
}
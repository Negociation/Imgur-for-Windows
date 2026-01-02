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
using System.Text;
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

        //-- Listagem de Itens Random (ViewModels)
        public ObservableCollection<MediaViewModel> RandomMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

        //-- Listagem de Itens Search "Tags" (ViewModels)
        public ObservableCollection<TagViewModel> TagsSearchMediaVmCollection { get; } = new ObservableCollection<TagViewModel>();

        //-- Listagem de Itens Search "Gallery" (ViewModels)
        public ObservableCollection<MediaViewModel> GallerySearchMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

        //-- Listagem de Itens Search "Users" (ViewModels)
        public ObservableCollection<AccountViewModel> UsersSearchMediaVmCollection { get; } = new ObservableCollection<AccountViewModel>();

        //-- Thumbnail Config Size
        public int ThumbSize => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);

        //-- LoadedSuccessfully state
        public bool LoadedSuccessfully => LoadedStaffPicks;

        //-- Nothing found during search state
        public bool NothingFound => !UsersAvaliableToShow && !GalleriesAvaliableToShow && !TagsAvaliableToShow;

        //-- There's Tags Avaliable to Show during Search
        public bool TagsAvaliableToShow => TagsSearchMediaVmCollection.Count() > 0;

        //-- There's Galleries Avaliable to Show during Search
        public bool GalleriesAvaliableToShow => GallerySearchMediaVmCollection.Count() > 0;

        //-- There's Galleries Avaliable to Show during Search
        public bool UsersAvaliableToShow => UsersSearchMediaVmCollection.Count() > 0;

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
            IAccountVmFactory accountVmFactory
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
        }

        public void InitializeSearch(string query)
        {
            this.Initialize();

            _searchQuery = query;
        }

        public void Initialize() {
            // Clear anterior ao carregar novos dados
            //StaffPicks = null;
    }

        public async Task InitializeAsync()
        {
            //Debug.WriteLine(StaffPicks.CurrentTag?.Items?.Count());
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


        //-- Command para Carregar Dados do Explorer (unificado)
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

                        // Executa a tarefa assíncrona em background
                        Task.Run(async () =>
                        {
                            try
                            {
                                // Delay para dar tempo da UI atualizar
                                await Task.Delay(10);
                                await RetrieveStaffPicksAsync();
                                await RetrieveRandomMediaAsync();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    //LoadingFailed = true;
                                });
                            }
                            finally
                            {
                                // Sempre desativa loading na UI thread
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

                // Buscar lista do service
                var retrivedTagContent = await _tagsService.GetStaffPicks();

                if (!retrivedTagContent.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrivedTagContent.Error);
                }

                // Cria ViewModel para Tag
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

                // Criar ViewModels
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

        //-- Command para Carregar Dados de Search (unificado)
        private ICommand _retrieveSearchDataCommand;

        public ICommand RetrieveSearchDataCommand
        {
            get
            {
                if (_retrieveSearchDataCommand == null)
                {
                    _retrieveSearchDataCommand = new RelayCommand<string>((query) =>
                    {
                        // Inicia Loading imediatamente
                        Loading = true;

                        // Executa a tarefa assíncrona em background
                        Task.Run(async () =>
                        {
                            try
                            {
                                // Delay para dar tempo da UI atualizar
                                await Task.Delay(10);
                                await RetrieveGalleriesAsync(query);
                                await Task.Delay(10);
                                await RetrieveTagsAsync(query);
                                await Task.Delay(10);
                                await RetrieveUsersAsync(query);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    //LoadingFailed = true;
                                });
                            }
                            finally
                            {
                                // Sempre desativa loading na UI thread
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    Loading = false;
                                    OnPropertyChanged(nameof(NothingFound));
                                });
                            }
                        });
                    });
                }
                return _retrieveSearchDataCommand;
            }
        }

        private async Task RetrieveGalleriesAsync(string query)
        {
            try
            {
                var retrievedGalleryMedia = await _galleryService.SearchGalleries(query);

                if (!retrievedGalleryMedia.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrievedGalleryMedia.Error);
                }

                var vmList = new List<MediaViewModel>();
                foreach (var media in retrievedGalleryMedia.Data)
                {
                    vmList.Add(_mediaVmFactory.GetMediaViewModel(media));
                }

                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    GallerySearchMediaVmCollection.Clear();
                    foreach (var vm in vmList.Take(20))
                        GallerySearchMediaVmCollection.Add(vm);

                    // ✅ Dentro do dispatcher
                    OnPropertyChanged(nameof(GallerySearchMediaVmCollection));
                    OnPropertyChanged(nameof(GalleriesAvaliableToShow));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task RetrieveTagsAsync(string query)
        {
            try
            {
                var retrievedTags = await _tagsService.SearchTags(query);

                if (!retrievedTags.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrievedTags.Error);
                }

                var vmList = new List<TagViewModel>();
                foreach (var item in retrievedTags.Data)
                {
                    if (item == null)
                    {
                        Debug.WriteLine("WARNING: null tag item");
                        continue;
                    }

                    vmList.Add(_tagVmFactory.GetTagViewModel(item));
                }

                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    
                    TagsSearchMediaVmCollection.Clear();
                    foreach (var vm in vmList.Take(20))
                        TagsSearchMediaVmCollection.Add(vm);
                   OnPropertyChanged(nameof(TagsSearchMediaVmCollection));
                   OnPropertyChanged(nameof(TagsAvaliableToShow));
                   
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task RetrieveUsersAsync(string query)
        {
            try
            {
                var retrievedUsers = await _accountService.SearchAccounts(query);

                if (!retrievedUsers.IsSuccess)
                {
                    throw new Exception("Erro durante a busca dos itens: " + retrievedUsers.Error);
                }

                var vmList = new List<AccountViewModel>();
                foreach (var item in retrievedUsers.Data)
                {
                    if (item == null)
                    {
                        Debug.WriteLine("WARNING: null tag item");
                        continue;
                    }

                    vmList.Add(_accountVmFactory.GetAccountViewModel(item));
                }

                _dispatcher.CheckBeginInvokeOnUi(() =>
                {

                    UsersSearchMediaVmCollection.Clear();
                    foreach (var vm in vmList.Take(20))
                        UsersSearchMediaVmCollection.Add(vm);
                    OnPropertyChanged(nameof(UsersSearchMediaVmCollection));
                    OnPropertyChanged(nameof(UsersAvaliableToShow));

                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
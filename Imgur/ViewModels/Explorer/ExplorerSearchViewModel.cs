using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Services;
using Imgur.ViewModels.Media;
using Imgur.ViewModels.Tags;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            }
        }

        //-- Loaded (Staff Picks Section) - Arrow Function
        public bool LoadedStaffPicks => _staffPicks != null;

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

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************
        public ExplorerSearchViewModel(
            IDispatcher dispatcher,
            INavigator navigator,
            TagsService tagsService,
            ITagVmFactory tagVmFactory,
            GalleryService galleryService,
            IMediaVmFactory mediaVmFactory
            )
        {
            _dispatcher = dispatcher;
            _navigator = navigator;
            _tagsService = tagsService;
            _tagVmFactory = tagVmFactory;
            _galleryService = galleryService;
            _mediaVmFactory = mediaVmFactory;
        }

        public void InitializeSearch(string query)
        {
            this.Initialize();

            _searchQuery = query;
        }

        public void Initialize()
        {
            _loading = true;

            // Clear anterior ao carregar novos dados
            //StaffPicks = null;
        }

        public async Task InitializeAsync()
        {
            Loading = true;

            if (IsExplorerMode)
            {
                if (RetrieveExplorerDataCommand.CanExecute(null)) { RetrieveExplorerDataCommand.Execute(null); }
                return;
            }

            if (IsSearchMode)
            {
                await Task.Delay(2000);
                Loading = false;
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
                                await Task.Delay(1000);
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
                    foreach (var vm in vmList)
                        RandomMediaVmCollection.Add(vm);
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
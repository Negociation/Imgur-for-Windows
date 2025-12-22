using Imgur.Api.Services.Models.Enum;
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
using System.Text;
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


        //-- Listagem de Itens (ViewModels)
        public ObservableCollection<MediaViewModel> RetrievedMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

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


        //-- Cancellation Token para Busca
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        //Current Page
        private int _currentPage = 0;

        //-- Status for Appending Current Media Info (Metadata only)
        private bool _isLoadingNewPage;

        //-- Status for Loading a new Page 
        public bool IsLoadingNewPage
        {
            get { return _isLoadingNewPage; }
            set
            {
                _isLoadingNewPage = value;
                OnPropertyChanged("IsLoadingNewPage");
            }
        }


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

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************
        public ExplorerViewModel(
            GalleryService galleryService,
            IDispatcher dispatcher,
            IMediaVmFactory mediaVmFactory,
            ILocalSettings localSettings,
            INavigator navigator
            )
        {
            _galleryService = galleryService;
            _dispatcher = dispatcher;
            _mediaVmFactory = mediaVmFactory;
            _localSettings = localSettings;
            _navigator = navigator;
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

        //-- Command para Busca de Itens do Explore
        private ICommand _retrieveGalleryContentCommand;

        public ICommand RetrieveGalleryContentCommand
        {
            get
            {
                if (_retrieveGalleryContentCommand == null)
                {
                    _retrieveGalleryContentCommand = new RelayCommand(() =>
                    {
                        // Inicia Loading imediatamente
                        Loading = true;
                        LoadedSuccessfully = true;
                        _currentPage = 0;

                        // Executa a tarefa assíncrona em background
                        Task.Run(async () =>
                        {
                            try
                            {
                                //Delay para tempo suficiente para atualizar sections
                                await Task.Delay(1000);

                                // Buscar lista do service
                                var retrievedMediaList = await _galleryService.GetExplorerMedia(Sections[SelectedSectionIndex].section,

                                    AvailableSorts[SelectedSortIndex].sort);

                                // Se deu erro
                                if (!retrievedMediaList.IsSuccess)
                                {
                                    throw new Exception("Erro durante a busca dos itens: " + retrievedMediaList.Error);
                                }

                                // Criar ViewModels
                                var vmList = new List<MediaViewModel>();
                                foreach (var media in retrievedMediaList.Data)
                                {
                                    vmList.Add(_mediaVmFactory.GetMediaViewModel(media));
                                }
                                // Atualizar UI thread
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    RetrievedMediaVmCollection.Clear();
                                    foreach (var vm in vmList)
                                        RetrievedMediaVmCollection.Add(vm);

                                    LoadedSuccessfully = true;
                                });

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                // Atualiza flag de sucesso na UI thread
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    LoadedSuccessfully = false;
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
                return _retrieveGalleryContentCommand;
            }
        }

        //-- Command para Busca de Itens do Explore
        private ICommand _loadMoreGalleryContentCommand;

        public ICommand LoadMoreGalleryContentCommand
        {
            get
            {
                if (_loadMoreGalleryContentCommand == null)
                {
                    _loadMoreGalleryContentCommand = new RelayCommand(() =>
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                // UI: iniciar loading
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    try
                                    {
                                        IsLoadingNewPage = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Erro UI (start loading): " + ex);
                                    }
                                });

                                // Delay visual
                                await Task.Delay(1000);

                                // Incrementar página
                                _currentPage += 1;

                                // Buscar lista
                                var retrievedMediaList =
                                    await _galleryService.GetExplorerMedia(
                                        Sections[SelectedSectionIndex].section,
                                        AvailableSorts[SelectedSortIndex].sort,
                                        _currentPage);

                                if (!retrievedMediaList.IsSuccess)
                                {
                                    // UI: finalizar loading em erro
                                    _dispatcher.CheckBeginInvokeOnUi(() =>
                                    {
                                        try
                                        {
                                            IsLoadingNewPage = false;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("Erro UI (stop loading - erro): " + ex);
                                        }
                                    });

                                    throw new Exception("Erro durante a busca dos itens: " +
                                                        retrievedMediaList.Error);
                                }

                                // Criar VMs fora da UI thread
                                var vmList = new List<MediaViewModel>();
                                foreach (var media in retrievedMediaList.Data)
                                {
                                    vmList.Add(_mediaVmFactory.GetMediaViewModel(media));
                                }

                                // UI: append dos itens
                                _dispatcher.CheckBeginInvokeOnUi(async () =>
                                {
                                    try
                                    {
                                        foreach (var vm in vmList)
                                        {
                                            RetrievedMediaVmCollection.Add(vm);
                                            await Task.Delay(10);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Erro UI (append itens): " + ex);
                                    }
                                    finally
                                    {
                                        try
                                        {
                                            IsLoadingNewPage = false;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("Erro UI (stop loading final): " + ex);
                                        }
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Erro background LoadMore: " + ex);

                                // Garantir que o loading não fique travado
                                _dispatcher.CheckBeginInvokeOnUi(() =>
                                {
                                    try
                                    {
                                        IsLoadingNewPage = false;
                                    }
                                    catch (Exception uiEx)
                                    {
                                        Debug.WriteLine("Erro UI (catch final): " + uiEx);
                                    }
                                });
                            }
                        });
                    });
                }

                return _loadMoreGalleryContentCommand;
            }
        }

        //***************************************************************
        // View Functions 
        //***************************************************************

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

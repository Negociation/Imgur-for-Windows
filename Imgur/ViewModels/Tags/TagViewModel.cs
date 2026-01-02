using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Tags
{
    public class TagViewModel : Observable
    {
        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- Current Tag to Show
        private Tag _currentTag;

        //-- Usuario Autenticado
        public bool IsAuthenticated => _userContext.IsAuthenticated;

        //-- Listagem de Itens (ViewModels)
        public ObservableCollection<MediaViewModel> RetrievedMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

        //-- Thumbnail Config Size
        public int ThumbSize => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);

        public IEnumerable<MediaViewModel> RetrievedMediaVmCollectionSmall => RetrievedMediaVmCollection.Take(40);

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

        //***************************************************************
        // Services 
        //***************************************************************

        //-- Service para Dialogos 
        private readonly IDialogService _dialogService;

        //-- Servico de Settings do App
        private readonly ILocalSettings _localSettings;

        //-- Service para Navegação 
        private readonly INavigator _navigator;

        //-- UserContext
        private readonly IUserContext _userContext;

        public Tag CurrentTag
        {
            get { return _currentTag; }
            set
            {
                _currentTag = value;
                OnPropertyChanged("CurrentTag");
            }
        }


        //***************************************************************
        // Constructors e Initializers
        //***************************************************************

        public TagViewModel(
            Tag tag,
            IMediaVmFactory _mediaVmFactory,
            IDialogService dialogService,
            IUserContext userContext,
            INavigator navigator,
            ILocalSettings localSettings)
        {
            CurrentTag = tag;

            if (tag.Items != null)
            {
                foreach (var media in tag.Items)
                    RetrievedMediaVmCollection.Add(_mediaVmFactory.GetMediaViewModel(media));
            }

            _dialogService = dialogService;
            _localSettings = localSettings;
            _userContext = userContext;
            _navigator = navigator;
        }


        //-- Command para seguir Tag
        private ICommand _followTagCommand;

        public ICommand FollowTagCommand
        {
            get
            {
                if (_followTagCommand == null)
                {
                    _followTagCommand = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.FollowTag);
                            return;
                        };
                    });
                }
                return _followTagCommand;
            }
        }

        //-- Command para abrir a midia
        private ICommand _navigateTagCommand;

        public ICommand NavigateTagCommand
        {
            get
            {
                if (_navigateTagCommand == null)
                {
                    _navigateTagCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(1000);
                        _navigator.Navigate("tag", this);
                    });
                }
                return _navigateTagCommand;
            }
        }

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
    }
}

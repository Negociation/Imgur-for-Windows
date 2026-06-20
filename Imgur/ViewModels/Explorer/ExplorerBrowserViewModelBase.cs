using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Helpers;
using System;
using System.Windows.Input;

namespace Imgur.ViewModels.Explorer
{
    public abstract class ExplorerBrowserViewModelBase : Observable
    {
        //***************************************************************
        // View Parameters
        //***************************************************************

        //-- Query
        private string _query;
        public string Query
        {
            get { return _query; }
            private set
            {
                _query = value;
                OnPropertyChanged(nameof(Query));
            }
        }

        //-- Loading Content Flag Property
        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }

        //***************************************************************
        // Services
        //***************************************************************

        protected readonly IDispatcher Dispatcher;
        protected readonly INavigator Navigator;

        //***************************************************************
        // Constructors
        //***************************************************************

        protected ExplorerBrowserViewModelBase(
            string query,
            IDispatcher dispatcher,
            INavigator navigator)
        {
            this.Query = query;
            Dispatcher = dispatcher;
            Navigator = navigator;
        }

        //***************************************************************
        // Shared Commands (esqueleto)
        //***************************************************************

        //-- Command de voltar à tela anterior
        private ICommand _goBackCommand;

        public ICommand LeaveCurrentPage
        {
            get
            {
                if (_goBackCommand == null)
                {
                    _goBackCommand = new RelayCommand(() =>
                    {
                        Navigator.GoBack();
                    });
                }
                return _goBackCommand;
            }
        }
    }
}
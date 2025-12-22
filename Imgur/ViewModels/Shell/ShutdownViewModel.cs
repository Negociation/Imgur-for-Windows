using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Imgur.Contracts;
using Imgur.Helpers;

namespace Imgur.ViewModels
{
    public class ShutdownViewModel : Observable
    {

        private INavigator _navigator;

        public ShutdownViewModel(INavigator navigator)
        {
            _navigator = navigator;
            this.StartAnimation = false;
        }

        private bool _startAnimation;

        public bool StartAnimation
        {
            get { return _startAnimation; }
            set
            {
                _startAnimation = value;
                OnPropertyChanged("StartAnimation");
            }
        }


        private ICommand _navigateToCommand;


        public ICommand NavigateToCommand
        {
            get
            {
                if (_navigateToCommand == null)
                {
                    _navigateToCommand = new RelayCommand(() => {
                        _navigator.Close();
                    });
                }
                return _navigateToCommand;
            }
        }


        private ICommand _initializeShutdownRoutine;

        public ICommand InitializeShutdownRoutine
        {
            get
            {
                if (_initializeShutdownRoutine == null)
                {
                    _initializeShutdownRoutine = new RelayCommand(() => {
                        this.StartAnimation = true;
                    });
                }
                return _initializeShutdownRoutine;
            }
        }

    }
}

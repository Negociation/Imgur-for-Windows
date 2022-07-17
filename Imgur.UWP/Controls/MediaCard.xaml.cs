﻿using Imgur.Services;
using Imgur.UWP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.UWP.Controls
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MediaCard : Page
    {
        public MediaCard()
        {
            this.InitializeComponent();
            /*
            ISystemInfoProvider systeminfo = (ISystemInfoProvider)App.Services.GetService(typeof(ISystemInfoProvider));

            if(systeminfo.IsMobile()){
                Debug.WriteLine("Mobile");
            }
            else
            {
                Debug.WriteLine("pc");
            }
            */
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.UWP.Controls
{
    public sealed partial class NavigationViewOld: UserControl
    {
        public NavigationViewOld()
        {
           // this.InitializeComponent();
            IsSelected = false;
            
        }




        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(NavigationViewOld), new PropertyMetadata(0));



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NavigationViewOld), new PropertyMetadata(0));




        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(NavigationViewOld), new PropertyMetadata(0));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set {
                    SetValue(IsSelectedProperty, value);
                    UpdateState();
                }
        }

        private void UpdateState(){
            Debug.WriteLine("UpdatedInside");

            if (IsSelected){
          //      VisualStateManager.GoToState(this, nameof(SelectedState), false);
            }else{
            //    VisualStateManager.GoToState(this, nameof(UnselectedState), false);
            }
        }


        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NavigationViewOld), new PropertyMetadata(0));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavigationViewOld), new PropertyMetadata(0));



        public FontFamily FontIcon
        {
            get { return (FontFamily)GetValue(FontIconProperty); }
            set { SetValue(FontIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontIconProperty =
            DependencyProperty.Register("FontIcon", typeof(FontFamily), typeof(NavigationViewOld), new PropertyMetadata(0));

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            IsSelected = true;
        }

    }
}

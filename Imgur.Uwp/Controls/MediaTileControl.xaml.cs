using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Imgur.Uwp.Controls
{
    public sealed partial class MediaTileControl : UserControl
    {
        public MediaTileControl()
        {
            this.InitializeComponent();

            this.Loaded += MediaTileControl_Loaded;
            this.Unloaded += MediaTileControl_Unloaded;
        }

        // =========================================================
        // VIEWMODEL
        // =========================================================
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(MediaViewModel),
                typeof(MediaTileControl),
                new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MediaTileControl)d;

            // Container reciclado para um novo item -> religa os indicadores.
            // (Best-effort: se o template ainda não foi materializado, o reset
            //  confiável acontece no Loaded e/ou o stop ocorre no ImageOpened/Failed.)
            if (e.NewValue != null)
                control.ResetLoadingIndicators();
        }

        public MediaViewModel ViewModel
        {
            get => (MediaViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // =========================================================
        // LIFECYCLE
        // =========================================================
        private void MediaTileControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Sem cover não há ImageOpened -> evita ring eterno.
            var cover = ViewModel?.CurrentMedia?.CoverImage;
            if (string.IsNullOrWhiteSpace(cover))
                StopLoadingIndicators();
        }

        private void MediaTileControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Com binding+converter, o release de bitmap é tratado pela virtualização
            // (rebind ao reciclar). Se quiser release agressivo, faça via
            // ContainerContentChanging (args.InRecycleQueue) no AdaptiveGridView.
        }

        // =========================================================
        // IMAGE / GIF LOADED OU FALHOU  -> PARA OS INDICADORES
        // =========================================================
        private void MediaThumb_ImageOpened(object sender, RoutedEventArgs e)
            => StopLoadingIndicators();

        private void MediaThumb_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[Tile] ImageFailed: " + e.ErrorMessage);
            StopLoadingIndicators();
        }

        private void MediaThumb_GifOpened(object sender, RoutedEventArgs e)
            => StopLoadingIndicators();

        private void MediaThumb_GifFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[Tile] GifFailed: " + e.ErrorMessage);
            StopLoadingIndicators();
        }

        // =========================================================
        // INDICADORES (ProgressRing + ProgressBar / overlay do GIF)
        // =========================================================
        private void StopLoadingIndicators()
        {
            // Para qualquer ProgressRing do template ativo (Image/Gif/Video).
            foreach (var ring in FindChildren<ProgressRing>(this))
            {
                ring.IsActive = false;
                ring.Visibility = Visibility.Collapsed;
            }

            // O overlay do GIF (gifPlaceholderContainer) contém a LoadingBar:
            // colapsar o container já interrompe a barra.
            var placeholder = FindChild<Grid>(this, "gifPlaceholderContainer");
            if (placeholder != null)
                placeholder.Visibility = Visibility.Collapsed;
        }

        private void ResetLoadingIndicators()
        {
            foreach (var ring in FindChildren<ProgressRing>(this))
            {
                ring.IsActive = true;
                ring.Visibility = Visibility.Visible;
            }

            var placeholder = FindChild<Grid>(this, "gifPlaceholderContainer");
            if (placeholder != null)
                placeholder.Visibility = Visibility.Visible;
        }

        // =========================================================
        // CONTEXT MENU
        // =========================================================
        private void Media_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            CommandBarFlyout1.ShowAt(MediaElement);
        }

        // =========================================================
        // SAFE VISUAL TREE SEARCH (LOCAL ONLY)
        // =========================================================
        private T FindChild<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.Name == name)
                    return element;

                var result = FindChild<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private List<T> FindChildren<T>(DependencyObject parent)
            where T : FrameworkElement
        {
            var results = new List<T>();
            CollectChildren(parent, results);
            return results;
        }

        private void CollectChildren<T>(DependencyObject parent, List<T> results)
            where T : FrameworkElement
        {
            if (parent == null) return;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element)
                    results.Add(element);

                CollectChildren(child, results);
            }
        }
    }
}
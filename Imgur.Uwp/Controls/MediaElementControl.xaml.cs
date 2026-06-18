using Imgur.Constants;
using Imgur.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.Uwp.Controls
{
    public sealed partial class MediaElementControl : UserControl
    {
        private readonly ILocalSettings _localSettings;
        private bool _settingsHooked;

        public MediaElementControl()
        {
            this.InitializeComponent();

            _localSettings = App.Services.GetRequiredService<ILocalSettings>();
            this.AutoPlay = _localSettings.Get<bool>(LocalSettingsConstants.AutoPlay);

            this.Loaded += MediaElementControl_Loaded;
            this.Unloaded += MediaElementControl_Unloaded;
        }

        private void MediaElementControl_Loaded(object sender, RoutedEventArgs e)
        {
            HookSettingsEvent();
            RestoreVisualSources();
        }

        private void MediaElementControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ReleaseVisualSources();
            UnhookSettingsEvent();
        }

        private void HookSettingsEvent()
        {
            if (_settingsHooked) return;

            _localSettings.SettingSet += OnSettingSet;
            _settingsHooked = true;
        }

        private void UnhookSettingsEvent()
        {
            if (!_settingsHooked) return;

            _localSettings.SettingSet -= OnSettingSet;
            _settingsHooked = false;
        }

        private void OnSettingSet(object sender, string settingsKey)
        {
            try
            {
                if (settingsKey == LocalSettingsConstants.AutoPlay)
                {
                    this.AutoPlay = _localSettings.Get<bool>(LocalSettingsConstants.AutoPlay);

                    if (!this.AutoPlay)
                        MediaElement.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void RestoreVisualSources()
        {
            try
            {
                // Quando AutoPlay=false, os elementos de vídeo podem nem existir (x:Load)
                if (!AutoPlay)
                {
                    if (MediaElement != null)
                        MediaElement.Stop();

                    return;
                }

                if (mediaPlaceholderImage != null)
                {
                    if (!string.IsNullOrWhiteSpace(VideoSourcePlaceholder))
                        mediaPlaceholderImage.Source = new BitmapImage(new Uri(VideoSourcePlaceholder));
                    else
                        mediaPlaceholderImage.Source = null;
                }

                if (MediaElement != null)
                {
                    if (!string.IsNullOrWhiteSpace(VideoSource))
                        MediaElement.Source = new Uri(VideoSource);
                    else
                        MediaElement.Source = null;

                    if (!AutoPlay)
                        MediaElement.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (mediaPlaceholderImage != null)
                    mediaPlaceholderImage.Source = null;

                if (MediaElement != null)
                    MediaElement.Source = null;
            }
        }

        private void ReleaseVisualSources()
        {
            try
            {
                if (MediaElement != null)
                {
                    MediaElement.Stop();
                    MediaElement.Source = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (mediaPlaceholderImage != null)
                mediaPlaceholderImage.Source = null;
        }

        private bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoPlay.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool), typeof(MediaElementControl), new PropertyMetadata(false));

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(MediaElementControl), new PropertyMetadata(null));

        public string VideoSource
        {
            get { return (string)GetValue(VideoSourceProperty); }
            set { SetValue(VideoSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoSourceProperty =
            DependencyProperty.Register("VideoSource", typeof(string), typeof(MediaElementControl), new PropertyMetadata(null));

        public string VideoSourcePlaceholder
        {
            get { return (string)GetValue(VideoSourcePlaceholderProperty); }
            set { SetValue(VideoSourcePlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoSourcePlaceholderProperty =
            DependencyProperty.Register("VideoSourcePlaceholder", typeof(string), typeof(MediaElementControl), new PropertyMetadata(null));
    }
}
using Imgur.Constants;
using Imgur.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.Uwp.Controls
{
    public sealed partial class MediaElementControl : UserControl
    {

        public MediaElementControl()
        {
            this.InitializeComponent();
            _localSettings = App.Services.GetRequiredService<ILocalSettings>();
            this.AutoPlay = _localSettings.Get<bool>(LocalSettingsConstants.AutoPlay);

            //Get Events
            _localSettings.SettingSet += OnSettingSet;


        }



        private ILocalSettings _localSettings;
        private void OnSettingSet(object sender, string settingsKey)
        {
            if (settingsKey == LocalSettingsConstants.AutoPlay)
            {
                this.AutoPlay = _localSettings.Get<bool>(LocalSettingsConstants.AutoPlay);
                if (!this.AutoPlay)
                {
                    MediaElement.Stop();
                }
            }
        }

        private bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoPlay.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool), typeof(MediaElementControl), new PropertyMetadata(0));



        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(MediaElementControl), new PropertyMetadata(0));


    }


}

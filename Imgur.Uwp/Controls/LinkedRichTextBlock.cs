using Imgur.Uwp.Helpers;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Imgur.Uwp.Controls
{
    /// <summary>
    /// RichTextBlock com parsing de URLs (flags de imagem/vídeo/link).
    /// Usável em DataTemplates via binding de Text.
    /// </summary>
    public sealed class LinkedRichTextBlock : UserControl
    {
        private readonly RichTextBlock _richText = new RichTextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };

        public LinkedRichTextBlock()
        {
            Content = _richText;
            RegisterPropertyChangedCallback(FontSizeProperty, (s, dp) => SyncTypography());
            RegisterPropertyChangedCallback(FontWeightProperty, (s, dp) => SyncTypography());
            RegisterPropertyChangedCallback(ForegroundProperty, (s, dp) => SyncTypography());
            Loaded += (s, e) =>
            {
                SyncTypography();
                ApplyRender();
            };
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(LinkedRichTextBlock),
                new PropertyMetadata(null, OnRenderPropsChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty RenderImagesProperty =
            DependencyProperty.Register(
                nameof(RenderImages),
                typeof(bool),
                typeof(LinkedRichTextBlock),
                new PropertyMetadata(true, OnRenderPropsChanged));

        public bool RenderImages
        {
            get => (bool)GetValue(RenderImagesProperty);
            set => SetValue(RenderImagesProperty, value);
        }

        public static readonly DependencyProperty RenderVideosProperty =
            DependencyProperty.Register(
                nameof(RenderVideos),
                typeof(bool),
                typeof(LinkedRichTextBlock),
                new PropertyMetadata(true, OnRenderPropsChanged));

        public bool RenderVideos
        {
            get => (bool)GetValue(RenderVideosProperty);
            set => SetValue(RenderVideosProperty, value);
        }

        public static readonly DependencyProperty RenderLinksProperty =
            DependencyProperty.Register(
                nameof(RenderLinks),
                typeof(bool),
                typeof(LinkedRichTextBlock),
                new PropertyMetadata(true, OnRenderPropsChanged));

        public bool RenderLinks
        {
            get => (bool)GetValue(RenderLinksProperty);
            set => SetValue(RenderLinksProperty, value);
        }

        private static void OnRenderPropsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LinkedRichTextBlock)d;
            control.SyncTypography();
            control.ApplyRender();
        }

        private void SyncTypography()
        {
            _richText.FontSize = FontSize;
            _richText.FontWeight = FontWeight;
            if (Foreground != null)
                _richText.Foreground = Foreground;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SyncTypography();
            ApplyRender();
        }

        private void ApplyRender()
        {
            CommentRenderer.Render(
                _richText,
                Text,
                RenderImages,
                RenderVideos,
                RenderLinks);
        }
    }
}

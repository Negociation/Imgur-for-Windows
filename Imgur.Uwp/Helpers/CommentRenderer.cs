using System;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Imgur.Uwp.Helpers
{
    /// <summary>
    /// Renderiza texto em RichTextBlock com links / embeds Imgur opcionais.
    /// Imagens (incl. WebP) via CompatibleImageLoader.
    /// Referências de álbum (#1, #2…) viram hyperlinks verdes (índice 0-based no callback).
    /// </summary>
    public static class CommentRenderer
    {
        private static readonly Regex TokenRegex = new Regex(
            @"(https?://[^\s<>""']+)|#(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly char[] TrailingUrlJunk = { '.', ',', ';', ':', ')', ']', '}', '"', '\'' };

        private static readonly SolidColorBrush HyperlinkBrush =
            new SolidColorBrush(Color.FromArgb(255, 0x1b, 0xb7, 0x6e));

        public static void Render(
            RichTextBlock target,
            string text,
            bool renderImages = true,
            bool renderVideos = true,
            bool renderLinks = true,
            Action<int> onAlbumPositionClick = null)
        {
            if (target == null)
                return;

            target.Blocks.Clear();

            var paragraph = new Paragraph();
            if (string.IsNullOrEmpty(text))
            {
                target.Blocks.Add(paragraph);
                return;
            }

            int lastIndex = 0;
            foreach (Match match in TokenRegex.Matches(text))
            {
                if (match.Index > lastIndex)
                    AddText(paragraph, text.Substring(lastIndex, match.Index - lastIndex));

                if (match.Groups[2].Success)
                {
                    // #N → callback com índice 0-based (N-1)
                    if (int.TryParse(match.Groups[2].Value, out var oneBased) && oneBased > 0)
                        AddAlbumPositionLink(paragraph, oneBased, onAlbumPositionClick);
                    else
                        AddText(paragraph, match.Value);
                }
                else
                {
                    var raw = match.Value;
                    var url = TrimTrailingJunk(raw);
                    var trailing = raw.Length > url.Length ? raw.Substring(url.Length) : string.Empty;

                    if (renderVideos && IsImgurMp4(url))
                    {
                        AddMediaEmbed(paragraph, CreateVideoElement(url), url, renderLinks);
                    }
                    else if (renderImages && IsEmbeddableImgurImage(url))
                    {
                        AddMediaEmbed(paragraph, CreateImageElement(url), url, renderLinks);
                    }
                    else if (renderLinks)
                    {
                        AddHyperlink(paragraph, url);
                    }
                    else
                    {
                        AddText(paragraph, url);
                    }

                    if (!string.IsNullOrEmpty(trailing))
                        AddText(paragraph, trailing);
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
                AddText(paragraph, text.Substring(lastIndex));

            target.Blocks.Add(paragraph);
        }

        private static void AddAlbumPositionLink(Paragraph paragraph, int oneBased, Action<int> onClick)
        {
            var hyperlink = new Hyperlink
            {
                Foreground = HyperlinkBrush,
                UnderlineStyle = UnderlineStyle.Single
            };
            hyperlink.Inlines.Add(new Run { Text = "#" + oneBased });

            if (onClick != null)
            {
                var zeroBased = oneBased - 1;
                hyperlink.Click += (s, e) => onClick(zeroBased);
            }

            paragraph.Inlines.Add(hyperlink);
        }

        private static void AddMediaEmbed(Paragraph paragraph, UIElement child, string fallbackUrl, bool renderLinks)
        {
            if (child == null)
            {
                if (renderLinks)
                    AddHyperlink(paragraph, fallbackUrl);
                else
                    AddText(paragraph, fallbackUrl);
                return;
            }

            if (paragraph.Inlines.Count > 0)
                paragraph.Inlines.Add(new LineBreak());

            paragraph.Inlines.Add(new InlineUIContainer { Child = child });
            paragraph.Inlines.Add(new LineBreak());
        }

        private static Image CreateImageElement(string url)
        {
            try
            {
                var image = new Image
                {
                    MaxWidth = 250,
                    MaxHeight = 250,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 4, 0, 4)
                };

                // BitmapImage preenchido async (WebP via BitmapDecoder do OS; demais via UriSource)
                image.Source = CompatibleImageLoader.CreateBitmap(url, 250);
                return image;
            }
            catch
            {
                return null;
            }
        }

        private static MediaElement CreateVideoElement(string url)
        {
            try
            {
                return new MediaElement
                {
                    Source = new Uri(url),
                    AreTransportControlsEnabled = false,
                    AutoPlay = true,
                    IsMuted = true,
                    IsLooping = true,
                    MaxWidth = 250,
                    MaxHeight = 250,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 4, 0, 4)
                };
            }
            catch
            {
                return null;
            }
        }

        private static void AddText(Paragraph paragraph, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            paragraph.Inlines.Add(new Run { Text = text });
        }

        private static void AddHyperlink(Paragraph paragraph, string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                paragraph.Inlines.Add(new Run { Text = url });
                return;
            }

            var hyperlink = new Hyperlink
            {
                Foreground = HyperlinkBrush,
                UnderlineStyle = UnderlineStyle.Single
            };
            hyperlink.Inlines.Add(new Run { Text = url });
            hyperlink.Click += async (s, e) =>
            {
                try
                {
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                catch
                {
                    // ignore launch failures
                }
            };
            paragraph.Inlines.Add(hyperlink);
        }

        private static string TrimTrailingJunk(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            return url.TrimEnd(TrailingUrlJunk);
        }

        private static bool IsImgurHost(Uri uri)
        {
            return uri.Host.EndsWith("i.imgur.com", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAllowedImageHost(Uri uri)
        {
            if (IsImgurHost(uri))
                return true;

            // Giphy CDN (webp e demais formatos de imagem)
            return uri.Host.Equals("media4.giphy.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(".giphy.com", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsImgurMp4(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !IsImgurHost(uri))
                return false;

            return uri.AbsolutePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEmbeddableImgurImage(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !IsAllowedImageHost(uri))
                return false;

            var path = uri.AbsolutePath;

            return path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        }
    }
}

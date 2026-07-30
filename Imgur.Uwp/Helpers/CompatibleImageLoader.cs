using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Imgur.Uwp.Helpers
{
    /// <summary>
    /// Carrega imagens para BitmapImage. WebP via BitmapDecoder do OS (WIC).
    /// </summary>
    public static class CompatibleImageLoader
    {
        private static readonly HttpClient Http = new HttpClient();
        private static readonly ConcurrentDictionary<string, byte[]> PngCache =
            new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Preenche um BitmapImage existente (padrão async-friendly para converter / rich text).
        /// </summary>
        public static async Task FillAsync(BitmapImage target, string url, int? maxWidth = null)
        {
            if (target == null || string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                if (!NeedsCompatibilityDecode(url))
                {
                    await RunOnUiAsync(() =>
                    {
                        target.DecodePixelType = DecodePixelType.Logical;
                        if (maxWidth.HasValue && maxWidth.Value > 0)
                            target.DecodePixelWidth = maxWidth.Value;
                        target.UriSource = new Uri(url);
                    }).ConfigureAwait(false);
                    return;
                }

                var pngBytes = await GetCompatiblePngBytesAsync(url, maxWidth).ConfigureAwait(false);
                if (pngBytes == null || pngBytes.Length == 0)
                    return;

                await RunOnUiAsync(async () =>
                {
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(pngBytes.AsBuffer());
                        stream.Seek(0);
                        await target.SetSourceAsync(stream);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CompatibleImageLoader] FillAsync failed: {ex}");
            }
        }

        /// <summary>
        /// Cria um BitmapImage e inicia o load async (converter / binding).
        /// </summary>
        public static BitmapImage CreateBitmap(string url, int? maxWidth = null)
        {
            var bmp = new BitmapImage();
            if (!string.IsNullOrWhiteSpace(url))
                _ = FillAsync(bmp, url, maxWidth);
            return bmp;
        }

        public static bool NeedsCompatibilityDecode(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return uri.AbsolutePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<byte[]> GetCompatiblePngBytesAsync(string url, int? maxWidth)
        {
            var cacheKey = url + "|" + (maxWidth ?? 0);
            if (PngCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var bytes = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
            if (bytes == null || bytes.Length == 0)
                return null;

            var png = await DecodeWebpToPngAsync(bytes, maxWidth).ConfigureAwait(false);
            if (png == null || png.Length == 0)
                return null;

            PngCache[cacheKey] = png;
            return png;
        }

        private static async Task<byte[]> DecodeWebpToPngAsync(byte[] webpBytes, int? maxWidth)
        {
            using (var inputStream = new InMemoryRandomAccessStream())
            {
                await inputStream.WriteAsync(webpBytes.AsBuffer());
                inputStream.Seek(0);

                var decoder = await BitmapDecoder.CreateAsync(inputStream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied);

                using (var outputStream = new InMemoryRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    if (maxWidth.HasValue && maxWidth.Value > 0 && softwareBitmap.PixelWidth > maxWidth.Value)
                    {
                        var scale = (double)maxWidth.Value / softwareBitmap.PixelWidth;
                        encoder.BitmapTransform.ScaledWidth = (uint)maxWidth.Value;
                        encoder.BitmapTransform.ScaledHeight = (uint)Math.Max(1, Math.Round(softwareBitmap.PixelHeight * scale));
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    }

                    await encoder.FlushAsync();
                    outputStream.Seek(0);

                    var reader = new DataReader(outputStream.GetInputStreamAt(0));
                    var size = (uint)outputStream.Size;
                    await reader.LoadAsync(size);
                    var png = new byte[size];
                    reader.ReadBytes(png);
                    return png;
                }
            }
        }

        private static async Task RunOnUiAsync(Action action)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher == null || dispatcher.HasThreadAccess)
            {
                action();
                return;
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        private static async Task RunOnUiAsync(Func<Task> action)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher == null || dispatcher.HasThreadAccess)
            {
                await action().ConfigureAwait(true);
                return;
            }

            var tcs = new TaskCompletionSource<object>();
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    await action().ConfigureAwait(true);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            await tcs.Task.ConfigureAwait(false);
        }
    }
}

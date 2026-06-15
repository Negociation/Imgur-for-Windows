using Imgur.Contracts;
using Imgur.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System;

namespace Imgur.Uwp.Services
{
    public class FilePicker : IFilePicker
    {
        // ← método estático reutilizável pelo App.xaml.cs
        public static async Task<SelectedFile> ReadStorageFileAsync(StorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);
            var bytes = new byte[buffer.Length];
            using (var reader = DataReader.FromBuffer(buffer))
                reader.ReadBytes(bytes);

            return new SelectedFile
            {
                Name = file.Name,
                Path = file.Path,
                MimeType = file.ContentType,
                Bytes = bytes
            };
        }

        public async Task<IReadOnlyList<SelectedFile>> PickMultipleFilesAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".webm");

            var files = await picker.PickMultipleFilesAsync().AsTask();
            var result = new List<SelectedFile>();
            if (files == null || files.Count == 0) return result;

            foreach (var file in files)
                result.Add(await ReadStorageFileAsync(file)); // ← reutiliza

            return result;
        }
    }
}
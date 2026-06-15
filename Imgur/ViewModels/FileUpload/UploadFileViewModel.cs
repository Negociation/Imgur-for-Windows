using Imgur.Api.Services.Actions;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using AlbumService = Imgur.Services.AlbumService;
using ImageService = Imgur.Services.ImageService;

namespace Imgur.ViewModels.FileUpload
{
    public class UploadFileViewModel : Observable
    {
        private readonly IFilePicker _filePicker;
        private readonly ISystemNotificationService _sysNotification;
        private readonly IAppNotificationService _appNotification;
        private readonly ImageUploadService _uploadImageService;
        private readonly AlbumService _albumService;
        private readonly ImageService _imageService;
        private readonly IMediaVmFactory _mediaVmFactory;

        // ← SelectedFile em vez de StorageFile
        private ObservableCollection<SelectedFile> _selectedFiles = new ObservableCollection<SelectedFile>();
        public ObservableCollection<SelectedFile> SelectedFiles
        {
            get { return _selectedFiles; }
            set {
                _selectedFiles = value;
                OnPropertyChanged("SelectedFiles");
                OnPropertyChanged("IsAlbum");
                OnPropertyChanged("HasFiles");
                OnPropertyChanged("IsReadyForUpload");
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }

        private bool _isPrivate;
        public bool IsPrivate
        {
            get { return _isPrivate; }
            set { _isPrivate = value; OnPropertyChanged("IsPrivate");  }
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get { return _isUploading; }
            set { _isUploading = value; OnPropertyChanged("IsUploading"); OnPropertyChanged("IsReadyForUpload"); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged("ErrorMessage"); }
        }

        public bool IsAlbum => _selectedFiles.Count > 1;
        public bool HasFiles => _selectedFiles.Count > 0;
        public bool IsReadyForUpload => HasFiles && !IsUploading;

        public Action OnUploadSuccess { get; set; }
        public Action OnUploadStarted { get; set; }
        public Action OnCancel { get; set; }

        public UploadFileViewModel(
            IFilePicker filePicker,
            ISystemNotificationService sysNotification,
            IAppNotificationService appNotification,
            IMediaVmFactory mediaVmFactory,
            ImageUploadService uploadImageService,
            AlbumService albumService,
            ImageService imageService
            )
        {
            _filePicker = filePicker;
            _uploadImageService = uploadImageService;
            _sysNotification = sysNotification;
            _albumService = albumService;
            _appNotification = appNotification;
            _imageService = imageService;
            _mediaVmFactory = mediaVmFactory;
        }

        //***************************************************************
        // Command — Abrir FileOpenPicker
        //***************************************************************
        private ICommand _openUploadPicker;
        public ICommand OpenUploadPicker
        {
            get
            {
                if (_openUploadPicker == null)
                {
                    _openUploadPicker = new RelayCommand(async () =>
                    {
                        var files = await _filePicker.PickMultipleFilesAsync();
                        if (files == null || files.Count == 0) return;

                        foreach (var file in files)
                        {
                            // ← compara por Name do SelectedFile
                            if (!_selectedFiles.Any(f => f.Name == file.Name))
                                _selectedFiles.Add(file);
                        }

                        OnPropertyChanged("SelectedFiles");
                        OnPropertyChanged("IsAlbum");
                        OnPropertyChanged("HasFiles");
                        OnPropertyChanged("IsReadyForUpload");
                    });
                }
                return _openUploadPicker;
            }
        }

        //***************************************************************
        // Command — Remover arquivo da lista
        //***************************************************************
        private ICommand _removeFileCommand;
        public ICommand RemoveFileCommand
        {
            get
            {
                if (_removeFileCommand == null)
                {
                    _removeFileCommand = new RelayCommand<SelectedFile>((file) =>
                    {
                        if (file == null) return;
                        _selectedFiles.Remove(file);
                        OnPropertyChanged("SelectedFiles");
                        OnPropertyChanged("IsAlbum");
                        OnPropertyChanged("HasFiles");
                        OnPropertyChanged("IsReadyForUpload");
                    });
                }
                return _removeFileCommand;
            }
        }

        //***************************************************************
        // Command — Upload
        //***************************************************************
        private ICommand _uploadCommand;
        public ICommand UploadCommand
        {
            get
            {
                if (_uploadCommand == null)
                {
                    _uploadCommand = new RelayCommand(async () =>
                    {
                        if (!HasFiles)
                        {
                            ErrorMessage = "Selecione ao menos um arquivo.";
                            return;
                        }

                        IsUploading = true;
                        OnUploadStarted?.Invoke();
                        ErrorMessage = null;

                        Debug.WriteLine($"[UploadViewModel] Iniciando upload de {_selectedFiles.Count} arquivo(s)...");
                        Debug.WriteLine($"[UploadViewModel] IsAlbum: {IsAlbum}");
                        Debug.WriteLine($"[UploadViewModel] Title: {Title}");

                        _sysNotification.ShowUploadInProgress(Title);

                        // ← FileUploadResult agora
                        var result = await _uploadImageService.UploadAsync(
                            _selectedFiles.ToList(),
                            Title ?? "",
                            Description ?? "", IsPrivate);

                        IsUploading = false;

                        if (result.IsSuccess)
                        {
                            Debug.WriteLine("Sucesso Upload");
                            _sysNotification.ShowUploadCompleted();

                            // ← usa FileUploadResult pra decidir o que buscar
                            if (result.Data.IsAlbum)
                            {

                             var album = await _albumService.GetAlbumById(result.Data.AlbumId);
                             if (album.IsSuccess)

                                 _appNotification.AddMediaClipboardNotification(
                                     _mediaVmFactory.GetMediaViewModel(album.Data),
                                     ImgurUrlType.Album, true);
                                     
                            }else{
                                var album = await _imageService.GetImageById(result.Data.ImageId);
                                {
                                    var image = await _imageService.GetImageById(result.Data.ImageId);
                                    if (image.IsSuccess)
                                        _appNotification.AddMediaClipboardNotification(
                                            _mediaVmFactory.GetMediaViewModel(image.Data),
                                            ImgurUrlType.Image, true);
                                }
                            }

                            OnUploadSuccess?.Invoke();
                        }
                        else
                        {
                            _sysNotification.ShowUploadFailed();
                            ErrorMessage = "Error";
                            Debug.WriteLine($"[Upload] Erro:");
                        }
                    });
                }
                return _uploadCommand;
            }
        }

        public void NotifyFilesChanged()
        {
            OnPropertyChanged(nameof(SelectedFiles));
            OnPropertyChanged(nameof(IsAlbum));
            OnPropertyChanged(nameof(HasFiles));
            OnPropertyChanged(nameof(IsReadyForUpload));
        }
    }
}
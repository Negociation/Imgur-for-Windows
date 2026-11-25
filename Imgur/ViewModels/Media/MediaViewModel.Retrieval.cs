using Imgur.Services;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Api.Services.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.Factories;
using System.Diagnostics;
using Imgur.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Imgur.ViewModels.Base;

namespace Imgur.ViewModels.Media
{
    public partial class MediaViewModel
    {
        //-- Command para Busca dos dados do Autor da Postagem
        private ICommand _retrieveUserCommand;
        public ICommand RetrieveUserCommand
        {
            get
            {
                if (_retrieveUserCommand == null)
                {
                    _retrieveUserCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            IsLoadingAuthor = true;
                            UserAccount = null;

                            if (!this.CurrentMedia.IsBasicAlbum)
                            {
                                if (!string.IsNullOrEmpty(this.CurrentMedia.AccountId))
                                {
                                    var account = await this._accountService.GetAccountById(this.CurrentMedia.AccountId);
                                    if (account.IsSuccess)
                                        this.UserAccount = this._accountVmFactory.GetAccountViewModel(account.Data);
                                    else
                                        throw new Exception("User Account couldn't be reached");
                                }
                                else
                                {
                                    var account = Imgur.Models.UserAccount.CreateAnonymous();
                                    this.UserAccount = this._accountVmFactory.GetAccountViewModel(account);
                                }
                            }
                            else
                            {
                                var account = Imgur.Models.UserAccount.CreateAnonymous();
                                this.UserAccount = this._accountVmFactory.GetAccountViewModel(account);
                            }

                            this.LoadedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            this.LoadedSuccessfully = false;
                            this.UserAccount = null;
                        }
                        finally
                        {
                            IsLoadingAuthor = false;
                        }
                    });
                }
                return _retrieveUserCommand;
            }
        }

        //-- Comando para refresh somente do Metadata da Postagem
        private ICommand _reloadCurrentMediaInfo;
        public ICommand ReloadCurrentMediaInfo
        {
            get
            {
                if (_reloadCurrentMediaInfo == null)
                {
                    _reloadCurrentMediaInfo = new RelayCommand(async () =>
                    {
                        try
                        {
                            ReloadingCurrentMediaInfo = true;

                            await Task.Delay(1000);

                            var hashId = this.CurrentMedia.Id;
                            if (!CurrentMedia.IsBasicAlbum)
                            {
                                var AlbumInfo = await _galleryService.GetGalleryAlbumById(hashId);

                                if (!AlbumInfo.IsSuccess)
                                    throw new Exception("Erro durante a busca dos metadados de album: " + AlbumInfo.Error);

                                if (NeedsElementsHydration())
                                    ApplyMediaContent(AlbumInfo.Data);
                                ApplyInteractiveMetadata(AlbumInfo.Data);
                            }
                            else
                            {
                                await RefreshAuthenticatedMediaStateAsync();
                            }
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            LoadedSuccessfully = false;
                        }
                        finally
                        {
                            ReloadingCurrentMediaInfo = false;
                        }
                    });
                }
                return _reloadCurrentMediaInfo;
            }
        }

        //-- Command para carregar demais itens de mídia
        private ICommand _loadRemaningItensCommand;
        public ICommand LoadRemaningItensCommand
        {
            get
            {
                if (_loadRemaningItensCommand == null)
                {
                    _loadRemaningItensCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(1000);
                        var ok = await EnsureRemainingAlbumElementsAsync();
                        if (!ok)
                            throw new Exception("Erro durante o carregamento dos elementos extras do album.");
                    });
                }
                return _loadRemaningItensCommand;
            }
        }

        /// <summary>
        /// Completa <see cref="Media.Elements"/> quando o álbum veio só com os primeiros itens.
        /// </summary>
        private async Task<bool> EnsureRemainingAlbumElementsAsync()
        {
            if (CurrentMedia == null || !CurrentMedia.IsAlbum)
                return true;

            if (!IsPartialLoading)
                return true;

            if (IsAppendingMediaData)
            {
                // Já há um load em curso (botão "carregar mais") — espera terminar.
                while (IsAppendingMediaData)
                    await Task.Delay(100);

                return !IsPartialLoading || (CurrentMedia.Elements?.Count ?? 0) > 0;
            }

            IsAppendingMediaData = true;
            try
            {
                var albumInfo = await _galleryService.GetGalleryAlbumById(CurrentMedia.Id);
                if (!albumInfo.IsSuccess || albumInfo.Data?.Elements == null)
                    return false;

                if (CurrentMedia.Elements == null)
                    CurrentMedia.Elements = new ObservableCollection<Element>();

                var already = CurrentMedia.Elements.Count;
                for (int i = already; i < albumInfo.Data.Elements.Count; i++)
                    CurrentMedia.Elements.Add(albumInfo.Data.Elements[i]);

                if (albumInfo.Data.ImagesCount.HasValue)
                    CurrentMedia.ImagesCount = albumInfo.Data.ImagesCount;

                OnPropertyChanged(nameof(IsPartialLoading));
                OnPropertyChanged(nameof(RemainingItemsCount));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM] EnsureRemainingAlbumElementsAsync: {ex.Message}");
                return false;
            }
            finally
            {
                IsAppendingMediaData = false;
            }
        }
    }
}

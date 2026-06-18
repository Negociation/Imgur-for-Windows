using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Base
{
    /// <summary>
    /// Classe base para ViewModels que suportam comentários e replies.
    /// Encapsula estado e commands comuns ao dialog de comentário.
    /// </summary>
    public abstract class CommentableViewModel : Observable
    {
        //***************************************************************
        // View Parameters — Comuns ao Dialog de Comentário
        //***************************************************************

        //-- Texto do comentário atual
        private string _commentText = string.Empty;
        public string CommentText
        {
            get => _commentText;
            set
            {
                if (_commentText == value) return;
                _commentText = value;
                OnPropertyChanged(nameof(CommentText));
                OnPropertyChanged(nameof(CommentCharCount));
                OnPropertyChanged(nameof(CommentCharCountInt));
                OnPropertyChanged(nameof(CanSubmitComment));
            }
        }

        //-- Contagem de caracteres como string para binding
        public string CommentCharCount => (CommentText?.Length ?? 0).ToString();

        //-- Contagem de caracteres como int para lógica interna
        public int CommentCharCountInt => CommentText?.Length ?? 0;

        //-- Acesso rápido ao estado de autenticação
        public bool IsAuthenticated => _userContext.IsAuthenticated;

        //-- Enviando comentário (bloqueia envio duplo)
        private bool _sendingComment;

        //-- Author para placeholder
        public string AuthorName => _userContext?.CurrentUser.Username;

        public bool SendingComment
        {
            get => _sendingComment;
            set
            {
                _sendingComment = value;
                OnPropertyChanged(nameof(SendingComment));
                OnPropertyChanged(nameof(CanSubmitComment));
            }
        }

        //-- Inferência de envio — texto preenchido e não enviando
        public bool CanSubmitComment => !string.IsNullOrWhiteSpace(CommentText)
                                     && CommentCharCountInt <= 500
                                     && !SendingComment;

        //-- Delegado disparado após comentário enviado com sucesso
        public Action OnCommentPosted { get; set; }

        //***************************************************************
        // Services — Injetados via construtor da subclasse
        //***************************************************************

        //-- Service para Dialogos 
        protected readonly IDialogService _dialogService;

        //-- UserContext
        protected readonly IUserContext _userContext;



        //-- Service para Envio de Notificações
        protected readonly IAppNotificationService _appNotification;

        //***************************************************************
        // Construtor
        //***************************************************************

        protected CommentableViewModel(
            IDialogService dialogService,
            IAppNotificationService appNotification,
            IUserContext userContext)
        {
            _dialogService = dialogService;
            _userContext = userContext;
            _commentText = string.Empty;
            _appNotification = appNotification;
        }

        //***************************************************************
        // Commands — Comuns
        //***************************************************************

        //-- Abre o dialog de comentário — implementação específica de cada subclasse
        private ICommand _startCommentCommand;
        public ICommand StartCommentCommand
        {
            get
            {
                if (_startCommentCommand == null)
                {
                    _startCommentCommand = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Comment);
                            return;
                        }
                        await OnStartCommentAsync();
                    });
                }
                return _startCommentCommand;
            }
        }

        //-- Envia o comentário — implementação específica de cada subclasse
        private ICommand _submitCommentCommand;
        public ICommand SubmitCommentCommand
        {
            get
            {
                if (_submitCommentCommand == null)
                {
                    _submitCommentCommand = new RelayCommand(async () =>
                    {
                        if (!CanSubmitComment || !IsAuthenticated) return;

                        SendingComment = true;

                        try
                        {
                            Debug.WriteLine($"[CommentableVM] Postando comentário: {CommentText}");
                            var success = await OnSubmitCommentAsync(CommentText);

                            if (success)
                            {
                                Debug.WriteLine("[CommentableVM] Comentário postado com sucesso.");
                                CommentText = string.Empty;

                                var notification = new NotificationViewModel();
                                notification.Message = "notification_comment_success_content";
                                this._appNotification.AddNotification(notification);

                                OnCommentPosted?.Invoke();
                            }
                            else
                            {
                                var notification = new NotificationViewModel();
                                notification.Message = "notification_comment_error_content";
                                this._appNotification.AddNotification(notification);
                            }
                        }
                        finally
                        {
                            SendingComment = false;
                        }
                    });
                }
                return _submitCommentCommand;
            }
        }

        //***************************************************************
        // Abstrações — cada subclasse implementa sua lógica
        //***************************************************************

        /// <summary>
        /// Chamado quando o usuário abre o dialog de comentário.
        /// MediaViewModel abre o dialog passando this.
        /// CommentViewModel abre o dialog de reply passando o comentário pai.
        /// </summary>
        protected abstract Task OnStartCommentAsync();

        /// <summary>
        /// Chamado quando o usuário submete o comentário.
        /// Retorna true se sucesso, false se erro.
        /// </summary>
        protected abstract Task<bool> OnSubmitCommentAsync(string text);
    }
}
using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Services;
using Imgur.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.ViewModels.Explorer
{
    public class ExplorerBrowserUsersViewModel : ExplorerBrowserViewModelBase
    {
        //***************************************************************
        // View Parameters
        //***************************************************************

        //-- Coleção incremental de Users (full — sem Take(30))
        public IIncrementalCollection<AccountViewModel> UsersVmCollection { get; private set; }

        //-- There's Users Available to Show
        public bool UsersAvailableToShow => (UsersVmCollection?.Count ?? 0) > 0;


        //***************************************************************
        // Services
        //***************************************************************

        private readonly AccountService _accountService;
        private readonly IAccountVmFactory _accountVmFactory;
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //***************************************************************
        // Constructor
        //***************************************************************

        /// <param name="preloadedPage0">
        /// Lista já carregada (Página 0) vinda do carrossel resumido da ExplorerSearchViewModel.
        /// Será inserida diretamente na coleção, evitando uma chamada de rede redundante.
        /// </param>
        public ExplorerBrowserUsersViewModel(
            IDispatcher dispatcher,
            INavigator navigator,
            AccountService accountService,
            IAccountVmFactory accountVmFactory,
            IIncrementalCollectionFactory collectionFactory,
            string query,
            List<AccountViewModel> preloadedPage0)
            : base(query, dispatcher, navigator)
        {
            _accountService = accountService;
            _accountVmFactory = accountVmFactory;
            _collectionFactory = collectionFactory;

            BuildCollection(preloadedPage0);
        }

        //***************************************************************
        // Collection Setup
        //***************************************************************

        private void BuildCollection(List<AccountViewModel> preloadedPage0)
        {
            UsersVmCollection = _collectionFactory.Create<AccountViewModel>(
                async (page, ct) =>
                {
                    // Página 0 já foi pré-carregada: devolve diretamente sem chamada de rede
                    if (page == 0)
                        return preloadedPage0 ?? new List<AccountViewModel>();

                    var result = await _accountService.SearchAccounts(Query, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<AccountViewModel>();

                    return result.Data
                        .Where(u => u != null)
                        .Select(_accountVmFactory.GetAccountViewModel)
                        .ToList();
                },
                pageSize: 60,
                batchSize: 12);

            UsersVmCollection.StateChanged += OnCollectionStateChanged;
            OnPropertyChanged(nameof(UsersVmCollection));
        }

        //***************************************************************
        // Initialization
        //***************************************************************

        public async Task InitializeAsync()
        {
            try
            {
                Loading = true;

                await UsersVmCollection.LoadNextPageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Loading = false;
                OnPropertyChanged(nameof(UsersAvailableToShow));
            }
        }

        private void OnCollectionStateChanged(object sender, EventArgs e)
        {
            Dispatcher.CheckBeginInvokeOnUi(async () =>
            {
                try
                {
                    if (!Loading && UsersVmCollection.CanLoadMorePages)
                    {
                        Loading = true;
                        var cts = new CancellationTokenSource();
                        var ct = cts.Token;

                        await UsersVmCollection.LoadNextPageAsync(ct);
                        OnPropertyChanged(nameof(UsersAvailableToShow));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro LoadMore: " + ex.Message);
                }
                finally
                {
                    Loading = false;
                }

            });
        }
    }
}
using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Imgur.Uwp.Services
{
    public class ShareService: IShareService
    {
        private string shareText;
        private string shareTitle;
        private string shareDescription;
        private DataTransferManager dataTransferManager;

        public void Initialize()
        {
            try
            {
                // Obtém o DataTransferManager da view atual
                dataTransferManager = DataTransferManager.GetForCurrentView();

                // Remove handler antigo se existir
                dataTransferManager.DataRequested -= OnDataRequested;

                // Adiciona o handler
                dataTransferManager.DataRequested += OnDataRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inicializar Share: {ex.Message}");
            }
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;

            if (!string.IsNullOrEmpty(shareText))
            {
                // Define o título que aparece no topo do Share
                request.Data.Properties.Title = shareTitle ?? "Compartilhar";

                // Define a descrição (opcional)
                if (!string.IsNullOrEmpty(shareDescription))
                {
                    request.Data.Properties.Description = shareDescription;
                }

                // Define o texto que será compartilhado
                request.Data.SetText(shareText);
            }
            else
            {
                request.FailWithDisplayText("Nenhum conteúdo disponível para compartilhar");
            }
        }


        /// <summary>
        /// Compartilha uma postagem com título e conteúdo
        /// </summary>
        public void ShareMediaPost(string tituloPostagem, string conteudoPostagem)
        {
            shareTitle = tituloPostagem;
            shareDescription = "Compartilhando postagem";
            shareText = $"{tituloPostagem}\n{conteudoPostagem}";

            ShowShareUI();
        }



        private void ShowShareUI()
        {
            try
            {
                // Este método funciona no Windows 10 Creators Update e superior
                // Abre a UI de compartilhamento do sistema
                DataTransferManager.ShowShareUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao mostrar Share UI: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            if (dataTransferManager != null)
            {
                dataTransferManager.DataRequested -= OnDataRequested;
            }
        }
    }
}

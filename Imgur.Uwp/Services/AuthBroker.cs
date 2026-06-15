using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Imgur.Uwp.Services
{
    public class AuthBroker : IAuthBroker
    {
        // Redirect URI que o Imgur redireciona corretamente
        private const string RedirectUri = "https://imgur.com/callback";

        public async Task<string> AuthenticateAsync(string authorizeUrl)
        {
            try
            {
                var startUri = new Uri(authorizeUrl);
                // URI nativo do pacote UWP — broker fecha automaticamente
                var callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

                Debug.WriteLine($"[AuthBroker] CallbackUri: {callbackUri}");
                Debug.WriteLine($"[AuthBroker] StartUri: {authorizeUrl}");

                var result = await WebAuthenticationBroker.AuthenticateAsync(
                    WebAuthenticationOptions.None,
                    startUri,
                    callbackUri);

                Debug.WriteLine("passou 2");
                switch (result.ResponseStatus)
                {
                    case WebAuthenticationStatus.Success:
                        System.Diagnostics.Debug.WriteLine(
                            $"[AuthBroker] ResponseData: {result.ResponseData}");
                        return result.ResponseData;

                    case WebAuthenticationStatus.UserCancel:
                        System.Diagnostics.Debug.WriteLine("[AuthBroker] Cancelado pelo usuário.");
                        return null;

                    case WebAuthenticationStatus.ErrorHttp:
                        System.Diagnostics.Debug.WriteLine(
                            $"[AuthBroker] HTTP Error: {result.ResponseErrorDetail}");
                        return null;

                    default:
                        System.Diagnostics.Debug.WriteLine(
                            $"[AuthBroker] ResponseData: {result.ResponseData}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthBroker] Exception: {ex.Message}");
                return null;
            }
        }
    }
}
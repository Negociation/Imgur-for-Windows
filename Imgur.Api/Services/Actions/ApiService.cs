using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services
{

    public abstract class ApiService: IApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _baseUrl = "https://api.imgur.com/3/";
        protected string _clientId;
        protected Func<string> _accessTokenProvider;

        protected ApiService(string clientId)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _clientId = clientId;
        }

        public void SetAccessTokenProvider(Func<string> provider)
        {
            _accessTokenProvider = provider;
        }

        protected void SetAuthHeader(HttpRequestMessage request, string accessToken = null)
        {
            var token = accessToken ?? _accessTokenProvider?.Invoke();
            if (!string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Fazendo Request via AccessToken" + token);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else if (!string.IsNullOrEmpty(_clientId))
            {
                Debug.WriteLine("Fazendo Request via ClientId" + _clientId);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Client-ID", _clientId);
            }
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string accessToken = null)
        {
            string json = null;
            HttpResponseMessage response = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
                {
                    SetAuthHeader(request, accessToken);
                    response = await _httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();

                    // Verifica se o status HTTP é de erro
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"HTTP Error {response.StatusCode} endpoint: {endpoint}");
                        return new ApiResponse<T>
                        {
                            Success = false,
                            Status = (int)response.StatusCode,
                            Data = default(T)
                        };
                    }

                    // Desserializa a resposta da API do Imgur
                    var result = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
                    return result;
                }
            }
            catch (JsonSerializationException ex)
            {
                Debug.WriteLine("Erro na desserialização JSON: " + ex.Message);
                Debug.WriteLine("Conteúdo do JSON: " + json);

                return new ApiResponse<T>
                {
                    Success = false,
                    Status = response?.StatusCode != null ? (int)response.StatusCode : 0,
                    Data = default(T)
                };
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Erro de rede: " + ex.Message);

                return new ApiResponse<T>
                {
                    Success = false,
                    Status = 0,
                    Data = default(T)
                };
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine("Timeout da requisição: " + ex.Message);

                return new ApiResponse<T>
                {
                    Success = false,
                    Status = 408, // Request Timeout
                    Data = default(T)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro genérico: " + ex.Message);
                Debug.WriteLine("Conteúdo do JSON: " + json);

                return new ApiResponse<T>
                {
                    Success = false,
                    Status = 0,
                    Data = default(T)
                };
            }
        }
        protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, HttpContent body = null)
        {
            string json = null;
            HttpResponseMessage response = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    SetAuthHeader(request);
                    request.Content = body;

                    response = await _httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"HTTP Error {response.StatusCode} endpoint: {endpoint}");
                        return new ApiResponse<T>
                        {
                            Success = false,
                            Status = (int)response.StatusCode,
                            Data = default(T)
                        };
                    }

                    return JsonConvert.DeserializeObject<ApiResponse<T>>(json);
                }
            }
            catch (JsonSerializationException ex)
            {
                Debug.WriteLine($"Erro na desserialização JSON: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = response != null ? (int)response.StatusCode : 0, Data = default(T) };
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erro de rede: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 0, Data = default(T) };
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"Timeout: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 408, Data = default(T) };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro genérico: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 0, Data = default(T) };
            }
        }

        protected async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            string json = null;
            HttpResponseMessage response = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Delete, endpoint))
                {
                    SetAuthHeader(request);
                    response = await _httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"HTTP Error {response.StatusCode} endpoint: {endpoint}");
                        return new ApiResponse<T>
                        {
                            Success = false,
                            Status = (int)response.StatusCode,
                            Data = default(T)
                        };
                    }

                    return JsonConvert.DeserializeObject<ApiResponse<T>>(json);
                }
            }
            catch (JsonSerializationException ex)
            {
                Debug.WriteLine($"Erro na desserialização JSON: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = response != null ? (int)response.StatusCode : 0, Data = default(T) };
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erro de rede: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 0, Data = default(T) };
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"Timeout: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 408, Data = default(T) };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro genérico: {ex.Message}");
                return new ApiResponse<T> { Success = false, Status = 0, Data = default(T) };
            }
        }
    }
}
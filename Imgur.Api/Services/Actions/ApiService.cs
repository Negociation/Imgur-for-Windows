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

    public abstract class ApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _baseUrl = "https://api.imgur.com/3/";
        protected string _clientId;
        protected string _accessToken;

        protected ApiService(string clientId)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _clientId = clientId;
        }

        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        protected void SetAuthHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            else if (!string.IsNullOrEmpty(_clientId))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Client-ID", _clientId);
            }
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            string json = null;
            HttpResponseMessage response = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
                {
                    SetAuthHeader(request);
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

        /*
        protected async Task<T> PostAsync<T>(string endpoint, object data = null)
        {
            SetAuthHeader();
            HttpContent content = null;

            if (data != null)
            {
                if (data is MultipartFormDataContent)
                {
                    content = data as MultipartFormDataContent;
                }
                else if (data is FormUrlEncodedContent)
                {
                    content = data as FormUrlEncodedContent;
                }
                else
                {
                    var json = JsonConvert.SerializeObject(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseJson);
            return result.Data;
        }

        protected async Task<T> PutAsync<T>(string endpoint, object data)
        {
            SetAuthHeader();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseJson);
            return result.Data;
        }

        protected async Task<bool> DeleteAsync(string endpoint)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BasicResponse>(json);
            return result.Data;
        }

        protected FormUrlEncodedContent CreateFormContent(Dictionary<string, string> data)
        {
            return new FormUrlEncodedContent(data);
        }

        protected async Task<MultipartFormDataContent> CreateMultipartContent(
            Dictionary<string, string> fields,
            StorageFile file = null)
        {
            var content = new MultipartFormDataContent();

            foreach (var field in fields)
            {
                if (!string.IsNullOrEmpty(field.Value))
                {
                    content.Add(new StringContent(field.Value), field.Key);
                }
            }

            if (file != null)
            {
                var stream = await file.OpenReadAsync();
                var streamContent = new StreamContent(stream.AsStreamForRead());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "image", file.Name);
            }

            return content;
        }
        */
    }
}
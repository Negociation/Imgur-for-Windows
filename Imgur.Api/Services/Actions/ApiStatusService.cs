using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.API.Resources.Status;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class ApiStatusService: ApiService, IApiStatusService
    {
        public ApiStatusService(string clientId) : base(clientId)
        {
        }

        public async Task<ApiResponse<Component>> GetApiStatusAsync()
        {
            var apiResponse = new ApiResponse<Component>();
            try
            {
                Root response = await this.GetRawAsync<Root>("https://status.imgur.com/api/v2/summary.json");
                apiResponse.Success = true;
                apiResponse.Status = 200;
                apiResponse.Data = response.components.Find(x => x.id == "m194673qcg7g"); //<-- Get Public API Status
            }
            catch (HttpRequestException ex)
            {
                apiResponse.Success = false;
                apiResponse.Status = (int)(((HttpResponseMessage)ex.InnerException.Data["response"]).StatusCode);
            }
            return (apiResponse);
        }

        private async Task<T> GetRawAsync<T>(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }



}

using Imgur.Api.Services.Models.Common;
using Imgur.API.Resources.Status;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IApiStatusService
    {
        Task<ApiResponse<Component>> GetApiStatusAsync();
    }
}

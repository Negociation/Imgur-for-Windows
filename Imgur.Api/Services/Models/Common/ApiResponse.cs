using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Common
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public int Status { get; set; }

    }

}

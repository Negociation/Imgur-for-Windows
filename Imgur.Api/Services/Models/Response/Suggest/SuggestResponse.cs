using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response.Suggest
{
    public class SuggestResponse
    {
        public List<TagsResponse> tags { get; set; }

        public List<UsersResponse> users { get; set; }
    }
}

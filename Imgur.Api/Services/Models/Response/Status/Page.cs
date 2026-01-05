using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.API.Resources.Status
{
    public class Page
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string time_zone { get; set; }
        public DateTime updated_at { get; set; }
    }

}

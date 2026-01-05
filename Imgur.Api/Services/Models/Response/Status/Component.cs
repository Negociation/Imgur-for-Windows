using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.API.Resources.Status
{
    public class Component
    {
        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public bool showcase { get; set; }
        public object start_date { get; set; }
        public string group_id { get; set; }
        public string page_id { get; set; }
        public bool group { get; set; }
        public bool only_show_if_degraded { get; set; }
        public List<string> components { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.API.Resources.Status
{
    public class Root
    {
        public Page page { get; set; }
        public List<Component> components { get; set; }
        public List<object> incidents { get; set; }
        public List<object> scheduled_maintenances { get; set; }
        public Status status { get; set; }
    }

}

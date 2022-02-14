using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class DialVideoCallVM
    {
        public string to { get; set; }
        public string from { get; set; }
        public string roomSid { get; set; }
        public string roomName { get; set; }
        public Boolean isVideo { get; set; }
    }
}

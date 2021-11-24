using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    class EmployeeCredential
    {
        public string firstname { get; set; }
        public string Lastname { get; set; }
        public string photograph { get; set; }
        public string personalemail { get; set; }
        public string officialemail { get; set; }
        public DateTime dob { get; set; }
        public string contact { get; set; }
        public string address { get; set; }
        public string gender { get; set; }
        public string martialstatus { get; set; }
        public string bloodgroup { get; set; }
        public string religion { get; set; }
        public string nationality { get; set; }
        public string empstatus { get; set; }
        public string created { get; set; }
        public string createdName { get; set; }

        public DateTime createdDate = new DateTime();
        public string modified { get; set; }
        public string modifiedName { get; set; }

        public DateTime modifiedDate = new DateTime();
        public string isDelete { get; set; }
    }
}

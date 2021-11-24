using System;

namespace Web.Model.Common
{
    public class RegisterCredential
    {
        public string fullname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public string created { get; set; }
        public string createdName { get; set; }

        public DateTime createdDate = new DateTime();
        public string modified { get; set; }
        public string modifiedName { get; set; }

        public DateTime modifiedDate = new DateTime();
        public string isDelete { get; set; }
    }
}

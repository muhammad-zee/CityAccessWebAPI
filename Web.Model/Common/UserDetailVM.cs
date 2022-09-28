using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class UserDetailVM
    {

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public int PartnerId { get; set; }
        public string PartnerTradeName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
   public class UserVM
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ConfirmPassword { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}

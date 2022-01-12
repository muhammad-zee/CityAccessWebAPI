using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services
{
    public class ApplicationSettings
    {
        public static int UserId { get; set; }
        public static string RoleIds { get; set; }
        public static string UserName { get; set; }
        public static bool isSuperAdmin { get; set; }
    }
}

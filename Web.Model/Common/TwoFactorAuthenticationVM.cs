using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class RequestTwoFactorAuthenticationCode
    {
        public int UserId { get; set; }
        public int SendCodeOn { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
    public class VerifyTwoFactorAuthenticationCode
    {
        public int UserId { get; set; }
        public string AuthenticationCode { get; set; }
        public DateTime? AuthenticationCodeExpireTime { get; set; }
        public int AuthenticationCodeExpiresInMinutes { get; set; }
        public string AuthenticationStatus { get; set; }
        public bool VerifyForThirtyDays { get; set; }
    }

}

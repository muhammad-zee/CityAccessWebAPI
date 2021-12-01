using System;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public DateTime? AuthenticationCodeExpireTime { get; set; }
        [JsonIgnore]
        public int AuthenticationCodeExpiresInMinutes { get; set; }
        [JsonIgnore]
        public string AuthenticationStatus { get; set; }
        public bool isVerifyForFuture { get; set; }
    }

}

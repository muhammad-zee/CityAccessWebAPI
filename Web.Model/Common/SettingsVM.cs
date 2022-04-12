using System;

namespace Web.Model.Common
{
    public class SettingsVM
    {
        public int SettingId { get; set; } = 0;
        public int? OrganizationIdFk { get; set; } = 0;
        public bool TwoFactorEnabled { get; set; }
        public int? TokenExpiryTime { get; set; } = 0;
        public string OrganizationEmail { get; set; }
        public int? TwoFactorCodeExpiry { get; set; } = 0;
        public int? VerifyCodeForFutureDays { get; set; } = 0;
        public int? PasswordLength { get; set; } 
        public bool RequiredLowerCase { get; set; }
        public bool RequiredUpperCase { get; set; }
        public bool RequiredNumeric { get; set; }
        public bool RequiredNonAlphaNumeric { get; set; }
        public int? EnablePasswordAge { get; set; }
        public int? CreatedBy { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}

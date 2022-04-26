using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class Setting
    {
        public int SettingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public bool TwoFactorEnable { get; set; }
        public int TokenExpiryTime { get; set; }
        public int TwoFactorAuthenticationExpiryMinutes { get; set; }
        public int VerifyForFutureDays { get; set; }
        public int? PasswordLength { get; set; }
        public bool RequiredLowerCase { get; set; }
        public bool RequiredUpperCase { get; set; }
        public bool RequiredNumeric { get; set; }
        public bool RequiredNonAlphaNumeric { get; set; }
        public int? EnablePasswordAge { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}

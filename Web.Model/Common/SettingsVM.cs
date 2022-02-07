using System;

namespace Web.Model.Common
{
    public class SettingsVM
    {
        public int SettingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int TokenExpiryTime { get; set; }
        public string OrganizationEmail { get; set; }
        public int TwoFactorCodeExpiry { get; set; }
        public int VerifyCodeForFutureDays { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}

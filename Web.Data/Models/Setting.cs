using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Setting
    {
        public int SettingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public bool TwoFactorEnable { get; set; }
        public int TokenExpiryTime { get; set; }
        public string OrganizationEmail { get; set; }
        public int TwoFactorAuthenticationExpiryMinutes { get; set; }
        public int VerifyForFutureDays { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}

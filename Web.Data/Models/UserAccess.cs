using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class UserAccess
    {
        public int UserAccessId { get; set; }
        public int UserIdFk { get; set; }
        public int RoleIdFk { get; set; }
        public int UserComIdFk { get; set; }
        public bool UserActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

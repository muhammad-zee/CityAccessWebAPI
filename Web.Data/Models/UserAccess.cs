using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class UserAccess
    {
        public int UserId { get; set; }
        public string UserRoleId { get; set; }
        public int? UserComId { get; set; }
        public bool UserActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

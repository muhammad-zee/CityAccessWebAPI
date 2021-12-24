using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class UsersRelation
    {
        public int UsersRelationId { get; set; }
        public int UserIdFk { get; set; }
        public int OrganizationIdFk { get; set; }
        public int DepartmentIdFk { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Organization
    {
        public Organization()
        {
            ClinicalHours = new HashSet<ClinicalHour>();
            OrganizationDepartments = new HashSet<OrganizationDepartment>();
            OrganizationRoles = new HashSet<OrganizationRole>();
        }

        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? OrganizationType { get; set; }
        public string PrimaryAddress { get; set; }
        public string PrimaryAddress2 { get; set; }
        public string PhoneNo { get; set; }
        public string PrimaryMobileNo { get; set; }
        public string PrimaryMobileNo2 { get; set; }
        public string FaxNo { get; set; }
        public string City { get; set; }
        public int? StateIdFk { get; set; }
        public string Zip { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<ClinicalHour> ClinicalHours { get; set; }
        public virtual ICollection<OrganizationDepartment> OrganizationDepartments { get; set; }
        public virtual ICollection<OrganizationRole> OrganizationRoles { get; set; }
    }
}

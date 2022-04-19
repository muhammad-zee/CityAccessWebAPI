using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class Organization
    {
        public Organization()
        {
            ActiveCodesNavigation = new HashSet<ActiveCode>();
            Departments = new HashSet<Department>();
            OrganizationConsultFields = new HashSet<OrganizationConsultField>();
        }

        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? OrganizationType { get; set; }
        public string OrganizationEmail { get; set; }
        public int? TimeZoneIdFk { get; set; }
        public string PrimaryAddress { get; set; }
        public string PrimaryAddress2 { get; set; }
        public string PhoneNo { get; set; }
        public string PrimaryMobileNo { get; set; }
        public string PrimaryMobileNo2 { get; set; }
        public string FaxNo { get; set; }
        public string City { get; set; }
        public int? StateIdFk { get; set; }
        public string Zip { get; set; }
        public string ActiveCodes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Setting Setting { get; set; }
        public virtual ICollection<ActiveCode> ActiveCodesNavigation { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<OrganizationConsultField> OrganizationConsultFields { get; set; }
    }
}

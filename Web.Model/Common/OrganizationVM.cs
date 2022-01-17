using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class OrganizationVM
    {
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
        public string OrgType { get; set; }
        public string State { get; set; }

        public int TimeZoneIdFk { get; set; }

        public string DepartmentIdsFk { get; set; }
        public List<DepartmentVM> Departments { get; set; }

        public List<clinicalHours> ClinicalHours { get; set; }
    }
}

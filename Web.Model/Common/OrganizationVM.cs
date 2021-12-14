﻿using System;

namespace Web.Model.Common
{
    public class OrganizationVM
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int? OrganizationType { get; set; }
        public string PrimaryAddress { get; set; }
        public string PrimaryAddress2 { get; set; }
        public decimal? PhoneNo { get; set; }
        public decimal? PrimaryMobileNo { get; set; }
        public decimal? PrimaryMobileNo2 { get; set; }
        public decimal? FaxNo { get; set; }
        public string City { get; set; }
        public int? StateIdFk { get; set; }
        public string Zip { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public string DepartmentIdsFk { get; set; }
    }
}
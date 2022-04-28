﻿using System;

namespace Web.Model.Common
{
    public class ServiceLineVM
    {
        public int ServiceLineId { get; set; }
        public string ServiceName { get; set; }
        public int? ServiceType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSelected { get; set; } = true;
        public bool IsOnCall { get; set; }

        public int? DepartmentIdFk { get; set; }
    }
}

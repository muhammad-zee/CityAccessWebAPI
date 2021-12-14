﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceLine
    {
        public ServiceLine()
        {
            DepartmentServices = new HashSet<DepartmentService>();
        }

        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int? ServiceType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<DepartmentService> DepartmentServices { get; set; }
    }
}
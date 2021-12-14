using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceLine
    {
        public int ServiceId { get; set; }
        public int? ServiceName { get; set; }
        public int? ServiceType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

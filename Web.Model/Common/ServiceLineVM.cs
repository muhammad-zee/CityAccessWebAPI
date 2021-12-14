using System;

namespace Web.Model.Common
{
    public class ServiceLineVM
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        //public int? ServiceType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        //public bool IsDeleted { get; set; }
    }
}

using System;

namespace Web.Model.Common
{
    public class IVRVM
    {
        public int IvrId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrganizationIdFk { get; set; }
        public int ServiceLineIdFk { get; set; }
        public string LandlineNumber { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}

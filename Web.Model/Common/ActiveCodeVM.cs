using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ActiveCodeVM
    {
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
        public int OrganizationIdFk { get; set; }
        public string OrganizationName { get; set; }
        public int CodeIdFk { get; set; }
        public string ServiceLineIds { get; set; }
        public int DefaultServiceLineId { get; set; }
        public string DefaultServiceLineName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public bool showAllActiveCodes { get; set; }
        public bool fromDashboard { get; set; }

        public List<ServiceLineVM> serviceLines { get; set; }
    }
}

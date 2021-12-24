using System;
using System.Collections.Generic;

namespace Web.Model.Common
{


    public class OrganizationSchedule
    {
        public int organizationId { get; set; }

        public int serviceId { get; set; }

        public int LoggedInUserId { get; set; }

        public List<clinicalHours> organizationHours { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }

    public class clinicalHours
    {
        public int id { get; set; }
        public int day { get; set; }
        public int ServicelineIdFk { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public DateTime? startBreak { get; set; }
        public DateTime? endBreak { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

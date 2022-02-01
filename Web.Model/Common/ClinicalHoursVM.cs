using System;
using System.Collections.Generic;

namespace Web.Model.Common
{


    public class OrganizationSchedule
    {


        public int clinicalHourId { get; set; }
        public int OrganizationIdFk { get; set; }

        public string serviceLineIdFk { get; set; }

        public string weekDays { get; set; }

        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        public string startTimeStr { get; set; }
        public string endTimeStr { get; set; }

        public int createdBy { get; set; }

        public int modifiedBy { get; set; }

    }

    public class clinicalHours
    {
        public int id { get; set; }
        public int day { get; set; }
        public int ServicelineIdFk { get; set; }
        public int OrganizationId { get; set; }
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
        public string WeekDay { get; set; }
        public List<ServiceLineVM> serviceLines { get; set; }
    }
}

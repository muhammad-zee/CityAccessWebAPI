using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class EditParams
    {
        public string key { get; set; }
        public string action { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        public string selectedServiceId { get; set; }
        public List<ScheduleEventData> added { get; set; }
        public List<ScheduleEventData> changed { get; set; }
        public List<ScheduleEventData> deleted { get; set; }
        public ScheduleEventData value { get; set; }
    }


    public class ScheduleEventData
    {

        public int Id { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
 
        public int OwnerId { get; set; }
        public string Subject { get; set; }

    }

    public class ScheduleVM
    {
        public int ScheduleId { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int OrganizationIdFk { get; set; }
        public string RoleIdFk { get; set; }
        public string UserIdFk { get; set; }
        public int DateRangeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }


        public int selectedOrganizationId { get; set; }
        public string selectedService { get; set; }
        public string selectedRole { get; set; }
        public string selectedUser { get; set; }
        public DateTime selectedFromDate { get; set; }
        public DateTime selectedToDate { get; set; }


    }
}

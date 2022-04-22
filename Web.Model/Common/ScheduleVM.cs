using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class EditParams
    {
        public string key { get; set; }
        public string action { get; set; }
        public string selectedUserId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int OrganizationId { get; set; }
        public string departmentIds { get; set; }
        public string ServiceLineIds { get; set; }
        public string RoleIds { get; set; }
        public string UserIds { get; set; }
        public string ShowAllSchedule { get; set; }
        public string ShowDepartmentSchedule { get; set; }
        public string ShowServiceLineSchedule { get; set; }
        public string ShowOnlyMySchedule { get; set; }
        public List<ScheduleEventData> added { get; set; }
        public List<ScheduleEventData> changed { get; set; }
        public List<ScheduleEventData> deleted { get; set; }
        public ScheduleEventData value { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
    }


    public class ScheduleEventData
    {
        public int id { get; set; }
        public DateTime endTime { get; set; }
        public DateTime startTime { get; set; }
        public string endTimeStr { get; set; }
        public string startTimeStr { get; set; }
        public string ownerId { get; set; }
        public string scheduleUserId { get; set; }
        public string userId { get; set; }

        public string serviceLineId { get; set; }
        public string departmentId { get; set; }
        public string roleId { get; set; }
        public string subject { get; set; }
        public string Description { get; set; }
        public string selectedUserId { get; set; }
        public string RoleName { get; set; }
        public string ServiceName { get; set; }
        public string DepartmentName { get; set; }

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

        public string FromDateStr { get; set; }
        public string ToDateStr { get; set; }
        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }

        public int RepeatEvery { get; set; }
        public string WeekDays { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<DateTime> SelectiveDates { get; set; }

        public int selectedOrganizationId { get; set; }
        public string selectedService { get; set; }
        public string selectedRole { get; set; }
        public string selectedUser { get; set; }
        public DateTime selectedFromDate { get; set; }
        public DateTime selectedToDate { get; set; }
        public int PageNumber { get; set; }
        public int Rows { get; set; }
        public string Filter { get; set; }
        public string SortOrder { get; set; }
        public string SortCol { get; set; }
        public string FilterVal { get; set; }

    }
}

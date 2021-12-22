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
        public string EndTimezone { get; set; }
        public bool IsAllDay { get; set; }
        public int OwnerId { get; set; }
        public string RecurrenceException { get; set; }
        public string Location { get; set; }
        public int? RecurrenceID { get; set; }
        public string StartTimezone { get; set; }
        public string Subject { get; set; }

    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Event
    {
        public Event()
        {
            Requests = new HashSet<Request>();
        }

        public int Id { get; set; }
        public int ServiceId { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxPersons { get; set; }
        public string StateId { get; set; }
        public string Notes { get; set; }

        public virtual Service Service { get; set; }
        public virtual EventState State { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
    }
}

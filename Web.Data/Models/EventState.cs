using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class EventState
    {
        public EventState()
        {
            Events = new HashSet<Event>();
        }

        public string Id { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}

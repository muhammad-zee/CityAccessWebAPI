using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class State
    {
        public State()
        {
            Requests = new HashSet<Request>();
        }

        public string Id { get; set; }

        public virtual ICollection<Request> Requests { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class MdrouteCounter
    {
        public string CounterName { get; set; }
        public string CounterInitial { get; set; }
        public long CounterValue { get; set; }
    }
}

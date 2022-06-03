using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class Temp
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

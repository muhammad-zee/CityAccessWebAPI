﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class TmpFerdeen
    {
        public DateTime? CreatedDate { get; set; }
        public int? UrgentConsults { get; set; }
        public int? RoutineConsults { get; set; }
    }
}

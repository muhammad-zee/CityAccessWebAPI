using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class StateTransition
    {
        public int Id { get; set; }
        public string Origin { get; set; }
        public string Destiny { get; set; }
    }
}

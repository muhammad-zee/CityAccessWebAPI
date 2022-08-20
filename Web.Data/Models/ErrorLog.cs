using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ErrorLog
    {
        public int Id { get; set; }
        public int User { get; set; }
        public DateTime ErrorDate { get; set; }
        public TimeSpan ErrorTime { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorUrl { get; set; }

        public virtual User UserNavigation { get; set; }
    }
}

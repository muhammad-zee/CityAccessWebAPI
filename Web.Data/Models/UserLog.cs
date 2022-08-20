using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class UserLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int EditorId { get; set; }
        public int UserId { get; set; }
        public string Notes { get; set; }

        public virtual User Editor { get; set; }
        public virtual User User { get; set; }
    }
}

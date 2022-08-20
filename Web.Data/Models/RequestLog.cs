using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class RequestLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string Notes { get; set; }

        public virtual Request Request { get; set; }
        public virtual User User { get; set; }
    }
}

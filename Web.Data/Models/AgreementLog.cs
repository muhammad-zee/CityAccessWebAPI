using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class AgreementLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int AgreementId { get; set; }
        public int UserId { get; set; }
        public string Notes { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual User User { get; set; }
    }
}

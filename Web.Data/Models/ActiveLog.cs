using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ActiveLog
    {
        public int ActiveLogId { get; set; }
        public long UserIdFk { get; set; }
        public string TableName { get; set; }
        public int TablePrimaryKey { get; set; }
        public int Action { get; set; }
        public string Changeset { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}

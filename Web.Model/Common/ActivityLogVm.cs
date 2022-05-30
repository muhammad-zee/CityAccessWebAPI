using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ActivityLogVm
    {
        public int ActiveLogId { get; set; }
        public long UserIdFk { get; set; }
        public string UserFullName { get; set; }
        public string TableName { get; set; }
        public int TablePrimaryKey { get; set; }
        public int Action { get; set; }
        public int ActionName { get; set; }
        public string Changeset { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

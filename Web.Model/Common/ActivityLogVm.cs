using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ActivityLogVm
    {
        public int ActivityLogId { get; set; }
        public long UserIdFk { get; set; }
        public string UserFullName { get; set; }
        public string TableName { get; set; }
        public int TablePrimaryKey { get; set; }
        public int Action { get; set; }
        public string ActionName { get; set; }
        public string Changeset { get; set; }
        public string PreviousValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }


    public class FilterActivityLogVM
    {
        public int CodeId { get; set; }
        public string ModuleId { get; set; }
        public string UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int LastRecordId { get; set; }
        public int PageSize { get; set; }
    }
}

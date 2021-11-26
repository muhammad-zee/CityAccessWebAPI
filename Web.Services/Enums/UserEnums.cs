using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services.Enums
{
    public enum UserEnums
    {
        [Description("Created")]
        Created = 'C',
        [Description("Updated")]
        Updated = 'U',
        [Description("Deleted")]
        Deleted = 'D',
        [Description("AlreadyCreated")]
        AlreadyCreated = 'X'
    }
}

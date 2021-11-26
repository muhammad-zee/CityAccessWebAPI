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
        Created = 1,
        [Description("Updated")]
        Updated = 2,
        [Description("Deleted")]
        Deleted = 3,
        [Description("AlreadyCreated")]
        AlreadyCreated = 4
    }
}

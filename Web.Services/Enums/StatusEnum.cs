using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services.Enums
{
    public enum StatusEnum
    {
        [Description("Success")]
        Success = 'S',
        [Description("Failed")]
        Failed = 'F',
        [Description("Exception")]
        Exception = 'E',
    }
}

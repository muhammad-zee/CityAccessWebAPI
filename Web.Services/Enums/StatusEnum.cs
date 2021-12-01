using System.ComponentModel;

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

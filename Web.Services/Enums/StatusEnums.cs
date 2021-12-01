using System.ComponentModel;

namespace Web.Services.Enums
{
    public enum StatusEnums
    {
        [Description("Success")]
        Success = 'S',
        [Description("Failed")]
        Failed = 'F',
        [Description("Exception")]
        Exception = 'E',
        [Description("Created")]
        Created = 'C',
        [Description("Updated")]
        Updated = 'U',
        [Description("Deleted")]
        Deleted = 'D',
        [Description("AlreadyExist")]
        AlreadyExist = 'X'
    }
}

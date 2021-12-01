using System.ComponentModel;

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

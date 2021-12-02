using System.ComponentModel;

namespace Web.Services.Enums
{
    public enum TwoFactorAuthenticationEnums
    {
        [Description("Send Code on SMS")]
        S = 'S',

        [Description("Send Code on Email")]
        E = 'E'
    }

}

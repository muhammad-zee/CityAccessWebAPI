using System.Linq;
using Web.Data.Models;
using System.Text;
using System.Threading.Tasks;
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

using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IJwtAuthService
    {
        BaseResponse Authentication(UserCredential login);

        string SaveUser(RegisterCredential register);
    }
}

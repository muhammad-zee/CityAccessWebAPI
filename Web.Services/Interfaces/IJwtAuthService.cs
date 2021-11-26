using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IJwtAuthService
    {
        BaseResponse Authentication(UserCredential login);

        int? Register(RegisterCredential register);
    }
}

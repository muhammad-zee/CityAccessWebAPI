using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IJwtAuthService
    {
        BaseResponse Authentication(UserCredentialVM login);
        BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication);
        string SaveUser(RegisterCredentialVM register);
        string SendResetPasswordMail(string userName, string url);
        string ResetPassword(UserCredentialVM credential);
    }
}

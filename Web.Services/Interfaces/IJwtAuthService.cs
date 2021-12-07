using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IJwtAuthService
    {
        BaseResponse Authentication(UserCredentialVM login);
        BaseResponse RefreshToken(int UserId);
        BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication);
        BaseResponse VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationCode verifyCode);
        BaseResponse ConfirmPassword(UserCredentialVM modelUser)
        string SaveUser(RegisterCredentialVM register);
        string SendResetPasswordMail(string userName, string url);
        string ResetPassword(UserCredentialVM credential);
    }
}

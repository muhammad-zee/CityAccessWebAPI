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
        BaseResponse ConfirmPassword(UserCredentialVM modelUser);
        BaseResponse SaveUser(RegisterCredentialVM register);
        BaseResponse AssociationUser(RegisterCredentialVM associate);
        string SendResetPasswordMail(string email, string url);
        string ResetPassword(UserCredentialVM credential);
    }
}

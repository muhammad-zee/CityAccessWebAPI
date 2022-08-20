using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IAuthService
    {
        BaseResponse Login(UserCredentialVM login);
        //BaseResponse Logout();
        //BaseResponse RefreshToken(int UserId);
        //BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication);
        //BaseResponse VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationCode verifyCode);
        //BaseResponse ConfirmPassword(UserCredentialVM modelUser);
        //BaseResponse SaveUser(RegisterCredentialVM register);
        //BaseResponse AssociationUser(RegisterCredentialVM associate);
        //BaseResponse AddOrUpdateFavouriteTeam(RegisterCredentialVM FavTeam);
        //BaseResponse ChangePassword(ChangePasswordVM changePassword);
        //string SendResetPasswordMail(string email, string url);
        //string ResetPassword(UserCredentialVM credential);
    }
}

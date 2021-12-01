﻿using Web.Model;
using Web.Model.Common;
namespace Web.Services.Interfaces
{
    public interface IJwtAuthService
    {
        BaseResponse Authentication(UserCredential login);
        BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication);
        string SaveUser(RegisterCredential register);
        string SendResetPasswordMail(string userName, string url);
        string ResetPassword(UserCredential credential);
    }
}

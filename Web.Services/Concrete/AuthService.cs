using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AuthService : IJwtAuthService
    {
        private readonly GenericRepository<User> _userRepo;
        private readonly GenericRepository<UserRole> _userRoleRepo;
        private readonly ICommunicationService _communicationService;
        private readonly IAdminService _adminService;
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        private string _encryptionKey = "";
        public AuthService(IConfiguration config,
            /*IUserAuthRepository userAuthRepository,*/
            IRepository<User> userRepo,
            IRepository<UserRole> userRoleRepo,
            ICommunicationService communicationService,
            IAdminService adminService)
        {
            this._config = config;
            //_userAuthRepository = userAuthRepository;
            this._userRepo = (GenericRepository<User>)userRepo;
            this._userRoleRepo = (GenericRepository<UserRole>)userRoleRepo;
            this._communicationService = communicationService;
            this._adminService = adminService;
            this._encryptionKey = this._config["Encryption:key"].ToString();

        }


        public BaseResponse Authentication(UserCredentialVM login)
        {
            BaseResponse response = new BaseResponse();
            if (!string.IsNullOrEmpty(login.username) && !string.IsNullOrEmpty(login.password))
            {
                var user = _userRepo.Table.Where(x => (x.UserName == login.username) && !x.IsDeleted).FirstOrDefault();
                if (user != null)
                {
                    login.password = Encryption.decryptData(login.password, this._encryptionKey);
                    user.Password = Encryption.decryptData(user.Password, this._encryptionKey);
                    if (user.Password == login.password)
                    {
                        var AuthorizedUser = GenerateJSONWebToken(user);
                        response.Body = AuthorizedUser; ;
                        response.Status = HttpStatusCode.OK;
                        response.Message = "User found";
                    }
                    else
                    {
                        response.Body = null;
                        response.Status = HttpStatusCode.NotFound;
                        response.Message = "Password is incorrect";
                    }
                }
                else
                {
                    response.Body = null;
                    response.Status = HttpStatusCode.NotFound;
                    response.Message = "Username is not valid";
                }
            }
            return response;
        }
        public BaseResponse RefreshToken(int UserId)
        {


            BaseResponse response = new BaseResponse();
            var user = _userRepo.Table.Where(u => u.UserId == UserId && !u.IsDeleted).FirstOrDefault();
            if (user != null)
            {
                var AuthorizationToken = GenerateJSONWebToken(user);
                response.Status = HttpStatusCode.OK;
                response.Message = "User found";
                response.Body = AuthorizationToken; ;
            }
            else
            {
                response.Body = null;
                response.Status = HttpStatusCode.NotFound;
                response.Message = "User Not Found";
            }
            return response;
        }
        private object GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.PrimaryEmail),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };
            var tokenExpiryTime = DateTime.UtcNow.AddMinutes(_config["Jwt:JwtExpiryTime"].ToInt());
            var token = new JwtSecurityToken(_config["Jwt:ValidIssuer"],
              _config["Jwt:ValidIssuer"],
              claims,
              expires: tokenExpiryTime,
              signingCredentials: credentials);

            //checking Verified for future or not 
            if (user.TwoFactorEnabled && user.IsTwoFactRememberChecked && DateTime.UtcNow <= user.TwoFactorExpiryDate)
            {
                user.TwoFactorEnabled = false;
            }
            List<UserRoleVM> UserRole = this._adminService.getRoleListByUserId(user.UserId).ToList();
            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires = tokenExpiryTime,
                PrimaryEmail = user.PrimaryEmail,
                PhoneNumber = user.PersonalMobileNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserId = user.UserId,
                Username = user.UserName,
                UserRole = UserRole,
                IsRequirePasswordReset = user.IsRequirePasswordReset

            };
        }

        public BaseResponse ConfirmPassword(UserCredentialVM modelUser)
        {
            var user = _userRepo.Table.Where(x => x.UserName == modelUser.username).FirstOrDefault();
            if (user != null)
            {
                user.IsRequirePasswordReset = false;
                user.Password = modelUser.password;
                _userRepo.Update(user);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Password Updated" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
            }
        }

        public string SaveUser(RegisterCredentialVM register)
        {
            if (register.UserId > 0)
            {
                var user = _userRepo.Table.Where(x => x.UserId == register.UserId && x.IsDeleted == false).FirstOrDefault();
                if (user.UserName != register.UserName)
                {
                    var alreadyExist = _userRepo.Table.Where(x => x.UserName == register.UserName && x.UserId != register.UserId).FirstOrDefault();
                    if (alreadyExist != null)
                    {
                        return StatusEnums.AlreadyExist.ToString();
                    }
                }
                user.FirstName = register.FirstName;
                user.MiddleName = register.MiddleName;
                user.LastName = register.LastName;
                user.PersonalMobileNumber = register.PersonalMobileNumber;
                user.UserName = register.UserName;
                user.PrimaryEmail = register.PrimaryEmail;
                user.StateKey = register.StateKey;
                user.Gender = register.GenderId.ToString();
                user.HomeAddress = register.HomeAddress;
                user.City = register.City;
                user.Zip = register.Zip;
                user.ModifiedBy = register.ModifiedBy;
                user.ModifiedDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(register.UserImage))
                {
                    var outPath = Directory.GetCurrentDirectory();
                    outPath += "/UserProfiles/";
                    if (!Directory.Exists(outPath))
                    {
                        Directory.CreateDirectory(outPath);
                    }
                    register.UserImageByte = Convert.FromBase64String(register.UserImage.Replace("data:image/jpeg;base64,", ""));
                    outPath += $"{user.FirstName}-{user.LastName}_{user.UserId}.png";
                    using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(register.UserImageByte);
                    }
                    user.UserImage = outPath;
                }
                _userRepo.Update(user);

                if (!string.IsNullOrEmpty(register.RoleIds))
                {
                    var roleIds = register.RoleIds.ToIntList();
                    var userRoles = _userRoleRepo.Table.Where(x => x.UserIdFk == register.UserId).ToList();
                    _userRoleRepo.DeleteRange(userRoles);

                    if (roleIds.Count() > 0)
                    {
                        List<UserRole> userRoleList = new List<UserRole>();
                        foreach (var item in roleIds)
                        {
                            userRoleList.Add(new UserRole() { UserIdFk = user.UserId, RoleIdFk = item });
                        }
                        _userRoleRepo.Insert(userRoleList);
                    }

                }

                return StatusEnums.Updated.ToString();
            }
            else
            {
                //first validate the username and password from the db and then generate token
                if (!string.IsNullOrEmpty(register.UserName) && !string.IsNullOrEmpty(register.Password)
                    && !string.IsNullOrEmpty(register.PrimaryEmail) && !string.IsNullOrEmpty(register.GenderId.ToString())
                    && !string.IsNullOrEmpty(register.RoleIds))
                {
                    var alreadyExist = _userRepo.GetList().Where(x => x.UserName == register.UserName && x.PrimaryEmail == register.PrimaryEmail && x.IsDeleted == false).FirstOrDefault();
                    if (alreadyExist == null)
                    {
                        var obj = new User()
                        {
                            FirstName = register.FirstName,
                            MiddleName = register.MiddleName,
                            LastName = register.LastName,
                            UserName = register.UserName,
                            Password = register.Password,
                            PrimaryEmail = register.PrimaryEmail,
                            PersonalMobileNumber = register.PersonalMobileNumber,
                            Gender = register.GenderId.ToString(),
                            HomeAddress = register.HomeAddress,
                            City = register.City,
                            Zip = register.Zip,
                            StateKey = register.StateKey,
                            CreatedBy = register.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            IsRequirePasswordReset = true
                        };
                        _userRepo.Insert(obj);
                        var roleIds = register.RoleIds.ToIntList();
                        List<UserRole> userRoleList = new List<UserRole>();
                        foreach (var item in roleIds)
                        {
                            userRoleList.Add(new UserRole() { UserIdFk = obj.UserId, RoleIdFk = item });
                        }
                        _userRoleRepo.Insert(userRoleList);
                        if (!string.IsNullOrEmpty(register.UserImage))
                        {
                            var outPath = Directory.GetCurrentDirectory();
                            outPath += "/UserProfiles/";
                            if (!Directory.Exists(outPath))
                            {
                                Directory.CreateDirectory(outPath);
                            }
                            outPath += $"{obj.FirstName}-{obj.LastName}_{obj.UserId}.png";
                            register.UserImageByte = Convert.FromBase64String(register.UserImage);
                            using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(register.UserImageByte);
                            }
                            var newAddedUser = _userRepo.Table.Where(x => x.UserId == obj.UserId).FirstOrDefault();
                            newAddedUser.UserImage = outPath;
                            _userRepo.Update(newAddedUser);
                        }
                        string sub = "Account Created.";
                        string mailBody = $"<b>Hi! {obj.FirstName}, </b><br />" +
                            $"<p>Your account is created.</p></br />" +
                            $"<p>Thanks!</p>";

                        this._communicationService.SendEmail(obj.PrimaryEmail, sub, mailBody, null);
                        return StatusEnums.Created.ToString();
                    }
                    else
                    {
                        return StatusEnums.AlreadyExist.ToString();
                    }
                }
            }
            return null;
        }

        #region Reset Password
        public string SendResetPasswordMail(string userName, string url)
        {
            try
            {
                var user = _userRepo.Table.Where(x => x.UserName == userName).FirstOrDefault();
                if (user != null)
                {
                    string Name = string.Empty;
                    string MrOrMrs = "Mr/Mrs.";
                    if (!string.IsNullOrEmpty(user.FirstName))
                    {
                        Name = user.FirstName;
                        if (!string.IsNullOrEmpty(user.LastName))
                        {
                            Name += user.LastName;
                        }
                    }
                    else
                    {
                        Name = new MailAddress(userName).User;
                    }
                    if (!string.IsNullOrEmpty(user.Gender))
                    {
                        if (user.Gender.Equals("Male"))
                        {
                            MrOrMrs = "Mr.";
                        }
                        else if (user.Gender.Equals("Female"))
                        {
                            MrOrMrs = "Mrs.";
                        }
                    }
                    string siteUrl = _config["siteUrl"];
                    string hashUserName = Encryption.encryptData(userName, this._encryptionKey);
                    string mailMessageTemplate = $"<b>Hi! {MrOrMrs} {Name},</b> <br />" +
                        $"<p>Please <a href='{siteUrl + hashUserName}' target='_blank'>Click here</a> to reset your password.</p> <br />" +
                        $"<p>If you didn’t ask to reset your password, you can ignore this email.</p> <br /><br />" +
                        $"<p>Thank You!</p>";
                    this._communicationService.SendEmail(user.PrimaryEmail, "Reset Password", mailMessageTemplate, null);

                    return StatusEnums.Success.ToString();
                }
            }
            catch (Exception ex)
            {
                //ElmahExtensions.RiseError(ex);
                return ex.ToString();
            }
            return null;
        }

        public string ResetPassword(UserCredentialVM credential)
        {
            try
            {
                var user = _userRepo.Table.Where(x => x.UserName == credential.username).FirstOrDefault();
                if (user != null)
                {
                    //var hashPswd = HelperExtension.Encrypt(credential.password);
                    var hashPswd = Encryption.encryptData(credential.password, this._encryptionKey);
                    user.Password = hashPswd;
                    _userRepo.Update(user);
                    return StatusEnums.Success.ToString();
                }
            }
            catch (Exception ex)
            {
                //ElmahExtensions.RiseError(ex);
                return ex.ToString();
            }

            return null;
        }

        #endregion

        #region Two Factor Atuthentication
        public BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication)
        {
            var user = _userRepo.Table.Where(u => u.UserId == Authentication.UserId).FirstOrDefault();

            var Authentication_Code_Sent = Send_Two_Factor_Authentication_Code(user, Authentication);
            VerifyTwoFactorAuthenticationCode responseBody;
            string ResponseMessage = "";
            if (Authentication_Code_Sent)
            {
                ResponseMessage = "Authentication Code Sent";
                responseBody = new VerifyTwoFactorAuthenticationCode
                {
                    UserId = user.UserId,
                    AuthenticationCode = user.TwoFactorCode,
                    AuthenticationCodeExpireTime = user.CodeExpiryTime,
                    AuthenticationCodeExpiresInMinutes = _config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt()
                };
            }
            else
            {
                ResponseMessage = "Authentication Code Not Sent";
                responseBody = null;
            }
            return new BaseResponse
            {
                Status = HttpStatusCode.OK,
                Message = ResponseMessage,
                Body = responseBody
            };
        }
        public bool Send_Two_Factor_Authentication_Code(User user, RequestTwoFactorAuthenticationCode Authentication)
        {
            try
            {
                string Two_Factor_Authentication_Code = GenerateTwoFactorAuthenticationCode();
                string Message_Body = "Routing and Queueing Two Factor Authentication Code: " + Two_Factor_Authentication_Code;
                bool Code_Sent = false;
                if (Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.S.ToString()))
                {
                    Code_Sent = this._communicationService.SendSms(user.PersonalMobileNumber, Message_Body);
                }
                else if (Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.E.ToString()))
                {
                    Code_Sent = this._communicationService.SendEmail(user.PrimaryEmail, "Authentication Code", Message_Body, null);
                }

                if (Code_Sent)
                {
                    user.TwoFactorCode = Two_Factor_Authentication_Code;
                    user.CodeExpiryTime = DateTime.UtcNow.AddMinutes(_config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt());
                    _userRepo.Update(user);
                }



                return Code_Sent;

            }
            catch (Exception ex)
            {
                //ElmahExtensions.RiseError(ex);
                return false;

            }
        }
        public string GenerateTwoFactorAuthenticationCode()
        {
            string chars = _config["TwoFactorAuthentication:TwoFactorAuthenticationCode"].ToString();
            var random = new Random();
            return new string(
            Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
        }

        public BaseResponse VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationCode verifyCode)
        {
            var user = _userRepo.Table.Where(u => u.UserId == verifyCode.UserId && !u.IsDeleted).FirstOrDefault();
            if (user != null)
            {
                if (verifyCode.AuthenticationCode == user.TwoFactorCode)
                {
                    if (verifyCode.isVerifyForFuture)
                    {
                        user.IsTwoFactRememberChecked = true;
                        user.TwoFactorExpiryDate = DateTime.UtcNow.AddDays(_config["TwoFactorAuthentication:VerifyForFutureDays"].ToInt());
                        _userRepo.Update(user);
                    }
                    verifyCode.AuthenticationStatus = "Verified";
                    return new BaseResponse { Status = HttpStatusCode.OK, Message = "Authentication Code Verified.", Body = verifyCode };
                }
                else
                {
                    verifyCode.AuthenticationStatus = "Not Verified";
                    return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "Authentication Code Mismatch.", Body = verifyCode };
                }

            }
            else
            {

                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
            }
        }

        #endregion
    }
}

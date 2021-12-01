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
using Twilio.AspNet.Common;
using Web.API.Helper;
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
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        public AuthService(IConfiguration config, /*IUserAuthRepository userAuthRepository,*/ IRepository<User> userRepo, IRepository<UserRole> userRoleRepo,ICommunicationService communicationService)
        {
            _config = config;
            //_userAuthRepository = userAuthRepository;
            _userRepo = (GenericRepository<User>)userRepo;
            _userRoleRepo = (GenericRepository<UserRole>)userRoleRepo;
            this._communicationService = communicationService;
        }


        public BaseResponse Authentication(UserCredentialVM login)
        {


            BaseResponse response = new BaseResponse();
            if (!string.IsNullOrEmpty(login.email) && !string.IsNullOrEmpty(login.password))
            {
                login.password = HelperExtension.Encrypt(login.password);
                var user = _userRepo.Table.Where(x => (x.PrimaryEmail == login.email || x.UserName == login.email) && x.Password == login.password).FirstOrDefault();
                if (user != null)
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
                    response.Message = "Email or password is not valid.";
                }
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

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires = tokenExpiryTime,
                PrimaryEmail = user.PrimaryEmail,
                PhoneNumber = user.PersonalMobileNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserId =user.UserId,
                Username = user.UserName


            };
        }

        public string SaveUser(RegisterCredentialVM register)
        {
            if (register.UserId > 0)
            {
                var user = AutoMapperHelper.MapSingleRow<RegisterCredentialVM, User>(register);
                _userRepo.Update(user);
                return UserEnums.Updated.ToString();
            }
            else
            {
                //first validate the username and password from the db and then generate token
                if (!string.IsNullOrEmpty(register.UserName) && !string.IsNullOrEmpty(register.Password)
                    && !string.IsNullOrEmpty(register.PrimaryEmail) && !string.IsNullOrEmpty(register.OfficePhoneNumber) && !string.IsNullOrEmpty(register.Gender)
                    && !string.IsNullOrEmpty(register.RoleIds))
                {
                    var alreadyExist = _userRepo.GetList().Where(x => x.UserName == register.UserName).FirstOrDefault();
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
                            OfficePhoneNumber = register.OfficePhoneNumber,
                            Gender = register.Gender,
                            CreatedBy = register.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        _userRepo.Insert(obj);
                        var roleIds = register.RoleIds.ToIntList();
                        List<UserRole> userRoleList = new List<UserRole>();
                        foreach (var item in roleIds)
                        {
                            userRoleList.Add(new UserRole() { UserId = obj.UserId, RoleId = item });
                        }
                        _userRoleRepo.Insert(userRoleList);
                        if (register.UserImageByte != null && register.UserImageByte.Count() > 0)
                        {
                            var outPath = Directory.GetCurrentDirectory();
                            outPath += "/UserProfiles/";
                            if (!Directory.Exists(outPath))
                            {
                                Directory.CreateDirectory(outPath);
                            }
                            outPath += $"UserProfilePic_{obj.UserId}.png";
                            using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(register.UserImageByte);
                            }
                            var newAddedUser = _userRepo.GetList().Where(x => x.UserId == obj.UserId).FirstOrDefault();
                            newAddedUser.UserImage = outPath;
                            _userRepo.Update(newAddedUser);
                        }

                        return UserEnums.Created.ToString();
                    }
                    else
                    {
                        return UserEnums.AlreadyCreated.ToString();
                    }

                    //unitorWork.Commit();

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
                    string hashUserName = HelperExtension.Encrypt(userName);
                    string mailMessageTemplate = $"<b>Hi! {MrOrMrs} {Name},</b> <br />" +
                        $"<p>Please <a href='{siteUrl + hashUserName}' target='_blank'>Click here</a> to reset your password.</p> <br />" +
                        $"<p>If you didn’t ask to reset your password, you can ignore this email.</p> <br /><br />" +
                        $"<p>Thank You!</p>";

                    return StatusEnum.Success.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return null;
        }

        public string ResetPassword(UserCredentialVM credential)
        {
            try
            {
                var user = _userRepo.Table.Where(x => x.UserName == credential.email).FirstOrDefault();
                if (user != null)
                {
                    var hashPswd = HelperExtension.Encrypt(credential.password);
                    user.Password = hashPswd;
                    _userRepo.Update(user);
                    return StatusEnum.Success.ToString();
                }
            }
            catch (Exception ex)
            {
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
                ResponseMessage = "Authentication Not Code Sent";
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
                if(Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.Sms.ToInt()))
                {
                    Code_Sent = this._communicationService.SendSms(user.PersonalMobileNumber, Message_Body);
                }
                else if (Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.Email.ToInt()))
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
            //var user = _userRepo.Table.Where(user)
            return null;
        }

        #endregion
    }
}

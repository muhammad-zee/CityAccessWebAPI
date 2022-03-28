﻿using Microsoft.AspNetCore.Hosting;
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
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<FavouriteTeam> _userFavouriteTeamRepo;
        private IRepository<ControlListDetail> _controlListDetails;
        private readonly ICommunicationService _communicationService;
        private readonly IAdminService _adminService;
        IConfiguration _config;
        private string _RootPath;
        private readonly UnitOfWork unitorWork;
        private string _encryptionKey = "";
        private IHostingEnvironment _environment;
        public AuthService(IConfiguration config,
            IHostingEnvironment environment,
            IRepository<User> userRepo,
            IRepository<UserRole> userRoleRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<FavouriteTeam> userFavouriteTeamRepo,
            IRepository<ControlListDetail> controlListDetails,
            ICommunicationService communicationService,
            IAdminService adminService)
        {
            this._config = config;
            this._environment = environment;
            this._userRepo = (GenericRepository<User>)userRepo;
            this._userRoleRepo = (GenericRepository<UserRole>)userRoleRepo;
            this._userRelationRepo = userRelationRepo;
            this._userFavouriteTeamRepo = userFavouriteTeamRepo;
            this._controlListDetails = controlListDetails;
            this._communicationService = communicationService;
            this._adminService = adminService;
            this._encryptionKey = this._config["Encryption:key"].ToString();
            this._RootPath = this._config["FilePath:Path"].ToString();
        }


        public BaseResponse Authentication(UserCredentialVM login)
        {
            BaseResponse response = new BaseResponse();
            if (!string.IsNullOrEmpty(login.username) && !string.IsNullOrEmpty(login.password))
            {
                var user = this._userRepo.Table.Where(x => (x.UserName == login.username) && x.IsDeleted != true).FirstOrDefault();
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.UserUniqueId))
                    {
                        var randomString = HelperExtension.CreateRandomString();
                        while (_userRepo.Table.Count(u => u.UserUniqueId == randomString && u.IsDeleted != true) > 0)
                        {
                            randomString = HelperExtension.CreateRandomString();
                        }
                        user.UserUniqueId = randomString;
                        this._userRepo.Update(user);
                    }

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
            var user = _userRepo.Table.Where(u => u.UserId == UserId && u.IsDeleted != true).FirstOrDefault();
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
            List<UserRoleVM> UserRole = this._adminService.getRoleListByUserId(user.UserId).ToList();
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.PrimaryEmail),
                    new Claim("UserId",user.UserId.ToString()),
                    new Claim("RoleIds",string.Join(",",UserRole.Select(x => x.RoleId).ToList())),
                    new Claim("isSuperAdmin",UserRole.Any(x =>x.RoleId == 2).ToString()),
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
            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires = tokenExpiryTime,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserId = user.UserId,
                UserFullName = user.FirstName + " " + user.LastName,
                PhoneNumber = user.PersonalMobileNumber,
                Username = user.UserName,
                PrimaryEmail = user.PrimaryEmail,
                UserRole = UserRole,
                IsRequirePasswordReset = user.IsRequirePasswordReset,
                NotificationChannelSid = user.UserChannelSid,
                conversationUserSid = user.ConversationUserSid,
                UserUniqueId = user.UserUniqueId,
                UserImage = user.UserImage,
                Gender = this._controlListDetails.Table.Where(x => x.ControlListDetailId == user.Gender.ToInt()).Select(x => x.Title).FirstOrDefault()
            };
        }

        public BaseResponse ConfirmPassword(UserCredentialVM modelUser)
        {
            var user = _userRepo.Table.Where(x => x.UserName == modelUser.username && x.IsDeleted != true).FirstOrDefault();
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

        public BaseResponse SaveUser(RegisterCredentialVM register)
        {

            if (register.UserId > 0)
            {
                var user = _userRepo.Table.Where(x => x.UserId == register.UserId && x.IsDeleted == false).FirstOrDefault();
                if (user.UserName != register.UserName)
                {
                    var alreadyExist = _userRepo.Table.Where(x => x.UserName == register.UserName && x.UserId != register.UserId).FirstOrDefault();
                    if (alreadyExist != null)
                    {
                        return new BaseResponse()
                        {
                            Status = HttpStatusCode.OK,
                            Message = "AlreadyExist",
                        };
                    }
                }

                if (register.DobStr != null)
                {
                    register.Dob = DateTime.Parse(register.DobStr);
                }

                user.FirstName = register.FirstName;
                user.MiddleName = register.MiddleName;
                user.LastName = register.LastName;
                user.PersonalMobileNumber = register.PersonalMobileNumber;
                user.UserName = register.UserName;
                user.PrimaryEmail = register.PrimaryEmail;
                user.StateKey = register.StateKey;
                user.Dob = register.Dob;
                user.Gender = register.GenderId.ToString();
                user.HomeAddress = register.HomeAddress;
                user.City = register.City;
                user.Zip = register.Zip;
                user.TwoFactorEnabled = register.TwoFactorEnabled;
                user.IsActive = register.IsActive;
                user.ModifiedBy = register.ModifiedBy;
                user.ModifiedDate = DateTime.UtcNow;
                user.IsEms = register.IsEMS;

                if (!string.IsNullOrEmpty(register.UserImage))
                {
                    //var outPath = Directory.GetCurrentDirectory();
                    var RootPath = this._RootPath; //this._environment.WebRootPath;
                    string FilePath = "UserProfiles";
                    var targetPath = Path.Combine(RootPath, FilePath);

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    register.UserImageByte = Convert.FromBase64String(register.UserImage.Split("base64,")[1]);
                    targetPath += "/" + $"{user.FirstName}-{user.LastName}_{user.UserId}.png";
                    using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(register.UserImageByte);
                    }
                    user.UserImage = targetPath.Replace(RootPath, "").Replace("\\", "/");
                }
                _userRepo.Update(user);

                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Updated",
                    Body = user.UserId,
                };
            }
            else
            {
                //first validate the username and password from the db and then generate token
                if (!string.IsNullOrEmpty(register.UserName) && !string.IsNullOrEmpty(register.Password)
                    && !string.IsNullOrEmpty(register.PrimaryEmail) && !string.IsNullOrEmpty(register.GenderId.ToString()))
                {
                    var alreadyExist = _userRepo.GetList().Where(x => x.UserName == register.UserName && x.PrimaryEmail == register.PrimaryEmail && x.IsDeleted == false).FirstOrDefault();
                    if (alreadyExist == null)
                    {
                        var randomString = HelperExtension.CreateRandomString();
                        while (_userRepo.Table.Count(u => u.UserUniqueId == randomString && !u.IsDeleted) > 0)
                        {
                            randomString = HelperExtension.CreateRandomString();
                        }

                        bool isMatched = true;
                        bool from2ndName = true;
                        int numOfLetters = 1;
                        while (isMatched)
                        {
                            if (_userRepo.Table.Any(x => x.Initials == register.Initials && !x.IsDeleted))
                            {
                                numOfLetters++;
                                if (numOfLetters <= (register.FirstName.Length) && numOfLetters <= (register.LastName.Length) && from2ndName)
                                {
                                    register.Initials = (register.FirstName.Substring(0, numOfLetters - 1) + register.LastName.Substring(0, numOfLetters)).ToUpper();
                                }
                                if (numOfLetters <= (register.FirstName.Length) && numOfLetters <= (register.LastName.Length) && !from2ndName)
                                {
                                    register.Initials = (register.FirstName.Substring(0, numOfLetters) + register.LastName.Substring(0, numOfLetters - 1)).ToUpper();
                                }
                                if (numOfLetters > (register.FirstName.Length) && numOfLetters > (register.LastName.Length))
                                {
                                    numOfLetters = 1;
                                    from2ndName = !from2ndName;
                                }
                            }
                            else
                            {
                                isMatched = false;
                            }
                        }


                        var notificationChannel = this._communicationService.createConversationChannel(register.FirstName + " " + register.LastName, randomString, "");
                        var conversationUser = this._communicationService.createConversationUser(randomString, register.FirstName + " " + register.LastName);
                        var addConversationUserToChannel = this._communicationService.addNewUserToConversationChannel(notificationChannel.Sid, randomString);
                        if (register.DobStr != null)
                        {
                            register.Dob = DateTime.Parse(register.DobStr);
                        }
                        var obj = new User()
                        {
                            FirstName = register.FirstName,
                            MiddleName = register.MiddleName,
                            LastName = register.LastName,
                            Initials = register.Initials,
                            UserName = register.UserName,
                            Password = register.Password,
                            PrimaryEmail = register.PrimaryEmail,
                            PersonalMobileNumber = register.PersonalMobileNumber,
                            Gender = register.GenderId.ToString(),
                            Dob = register.Dob,
                            HomeAddress = register.HomeAddress,
                            City = register.City,
                            Zip = register.Zip,
                            TwoFactorEnabled = register.TwoFactorEnabled,
                            IsActive = register.IsActive,
                            StateKey = register.StateKey,
                            CreatedBy = register.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            UserUniqueId = randomString,
                            UserChannelSid = notificationChannel.Sid,
                            ConversationUserSid = conversationUser.Sid,
                            IsDeleted = false,
                            IsRequirePasswordReset = true,
                            IsEms = register.IsEMS
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
                            var RootPath = this._RootPath; //this._environment.WebRootPath;
                            string FilePath = "UserProfiles";
                            var targetPath = Path.Combine(RootPath, FilePath);
                            if (!Directory.Exists(targetPath))
                            {
                                Directory.CreateDirectory(targetPath);
                            }
                            targetPath += "/" + $"{obj.FirstName}-{obj.LastName}_{obj.UserId}.png";
                            register.UserImageByte = Convert.FromBase64String(register.UserImage.Split("base64,")[1]);
                            using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(register.UserImageByte);
                            }
                            var newAddedUser = _userRepo.Table.Where(x => x.UserId == obj.UserId).FirstOrDefault();
                            newAddedUser.UserImage = targetPath.Replace(RootPath, "").Replace("\\", "/"); ;
                            _userRepo.Update(newAddedUser);
                        }

                        string sub = "Account Created.";
                        string mailBody = $"<b>Hi! {obj.FirstName}, </b><br />" +
                            $"<p>Your account is created.</p></br />" +
                            $"<p>Thanks!</p>";

                        this._communicationService.SendEmail(obj.PrimaryEmail, sub, mailBody, null);
                        return new BaseResponse()
                        {
                            Status = HttpStatusCode.OK,
                            Message = "Created",
                            Body = obj.UserId,
                        };
                    }
                    else
                    {
                        return new BaseResponse()
                        {
                            Status = HttpStatusCode.OK,
                            Message = "AlreadyExist",
                        };
                    }
                }
            }
            return null;
        }

        public BaseResponse AssociationUser(RegisterCredentialVM associate)
        {
            //var orgIds = associate.orgIds.ToIntList();
            //var dptIds = associate.dptIds.ToIntList();
            var serviceLineIds = associate.serviceIds.ToIntList();

            List<UsersRelation> userRelations = new List<UsersRelation>();

            if (!string.IsNullOrEmpty(associate.RoleIds))
            {
                var roleIds = associate.RoleIds.ToIntList();
                var userRoles = _userRoleRepo.Table.Where(x => x.UserIdFk == associate.UserId).ToList();
                _userRoleRepo.DeleteRange(userRoles);

                if (roleIds.Count() > 0)
                {
                    List<UserRole> userRoleList = new List<UserRole>();
                    foreach (var item in roleIds)
                    {
                        userRoleList.Add(new UserRole() { UserIdFk = associate.UserId, RoleIdFk = item });
                    }
                    _userRoleRepo.Insert(userRoleList);
                }

            }

            var relation = _userRelationRepo.Table.Where(x => x.UserIdFk == associate.UserId && x.IsDeleted != true).ToList();
            if (relation.Count() > 0)
            {
                _userRelationRepo.DeleteRange(relation);
            }

            foreach (var item in serviceLineIds)
            {
                var userRelation = new UsersRelation()
                {
                    UserIdFk = associate.UserId,
                    ServiceLineIdFk = item,
                    CreatedBy = associate.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                };

                userRelations.Add(userRelation);
            }

            if (userRelations.Count > 0)
            {
                _userRelationRepo.Insert(userRelations);
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Saved",
            };
        }

        public BaseResponse AddOrUpdateFavouriteTeam(RegisterCredentialVM FavTeam)
        {
            var serviceLineIds = FavTeam.serviceIds.ToIntList();

            List<FavouriteTeam> favTeam = new List<FavouriteTeam>();

            var relation = _userFavouriteTeamRepo.Table.Where(x => x.UserIdFk == FavTeam.UserId && x.IsDeleted != true).ToList();
            if (relation.Count() > 0)
            {
                _userFavouriteTeamRepo.DeleteRange(relation);
            }

            foreach (var item in serviceLineIds)
            {
                var userRelation = new FavouriteTeam()
                {
                    UserIdFk = FavTeam.UserId,
                    ServiceLineIdFk = item,
                    CreatedBy = FavTeam.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                };

                favTeam.Add(userRelation);
            }

            if (favTeam.Count > 0)
            {
                _userFavouriteTeamRepo.Insert(favTeam);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Saved",
                };
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "No Record Saved",
            };
        }


        #region Reset Password

        public BaseResponse ChangePassword(ChangePasswordVM changePassword)
        {
            if (changePassword.isFromProfile)
            {
                var user = _userRepo.Table.Where(x => !x.IsDeleted && x.UserId == changePassword.UserId).FirstOrDefault();
                var userOldPass = Encryption.decryptData(user.Password, this._encryptionKey);
                var oldPass = Encryption.decryptData(changePassword.OldPassword, this._encryptionKey);
                if (userOldPass == oldPass)
                {
                    user.Password = changePassword.NewPassword;
                    user.ModifiedBy = ApplicationSettings.UserId;
                    user.ModifiedDate = DateTime.UtcNow;
                    _userRepo.Update(user);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Password Change Successfully" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Old Password is not correct. Please write correct old password" };
                }
            }
            else
            {
                var user = _userRepo.Table.Where(x => !x.IsDeleted && x.UserId == changePassword.UserId).FirstOrDefault();
                user.Password = changePassword.NewPassword;
                user.ModifiedBy = ApplicationSettings.UserId;
                user.ModifiedDate = DateTime.UtcNow;
                _userRepo.Update(user);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Password Change Successfully" };
            }
        }

        public string SendResetPasswordMail(string email, string url)
        {
            try
            {
                var user = _userRepo.Table.Where(x => x.PrimaryEmail == email).FirstOrDefault();
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
                        Name = new MailAddress(email).User;
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
                    //string siteUrl = _config["siteUrl"];
                    string hashUserName = Encryption.encryptData(user.UserName, this._encryptionKey);
                    string mailMessageTemplate = $"<b>Hi! {MrOrMrs} {Name},</b> <br />" +
                        $"<p>Please <a href='{url + "/" + hashUserName}' target='_blank'>Click here</a> to reset your password.</p> <br />" +
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
                    //var hashPswd = Encryption.encryptData(credential.password, this._encryptionKey);
                    user.Password = credential.password;
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
    public class AuthService : IAuthService
    {
        private CityAccess_DbContext _dbContext;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Partner> _partnerRepo;

        IConfiguration _config;
        private string _RootPath;
        private string _encryptionKey = "";
        private IHostingEnvironment _environment;
        public AuthService(IConfiguration config,
            CityAccess_DbContext dbContext,
            IHostingEnvironment environment,
            IGenericRepository<User> userRepo,
            IGenericRepository<Partner> partnerRepo)
        {
            this._config = config;
            this._environment = environment;
            this._userRepo = userRepo;
            this._partnerRepo = partnerRepo;

            this._encryptionKey = this._config["Encryption:key"].ToString();
            this._RootPath = this._config["FilePath:Path"].ToString();
            this._dbContext = dbContext;
        }


        public BaseResponse Login(UserCredentialVM login)
        {
            BaseResponse response = new BaseResponse();
            if (!string.IsNullOrEmpty(login.username) && !string.IsNullOrEmpty(login.password))
            {
                var user = this._userRepo.Table.Where(x => (x.Username == login.username) && x.IsActive != false).FirstOrDefault();
                if (user != null)
                {
                    string encryptedPassword = login.password;
                    login.password = Encryption.MD5Hash(login.password);
                    if (user.Password == login.password)
                    {
                        var AuthorizedUser = GenerateJSONWebToken(user);
                        response.Body = AuthorizedUser;
                        response.Status = HttpStatusCode.OK;
                        response.Message = "User found";
                        ApplicationSettings.UserFullName = user.FullName;
                        //this._dbContext.Log(login, "Users", user.UserId, ActivityLogActionEnums.SignIn.ToInt());
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


        private object GenerateJSONWebToken(User user)
        {
            var partner = this._partnerRepo.Table.FirstOrDefault(x => x.Id == user.PartnerId && x.IsActive != false);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim("UserId",user.Id.ToString()),
                    new Claim("UserFullName", $"{user.FullName}"),
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
                UserId = user.Id,
                UserFullName = user.FullName,
                PhoneNumber = user.Phone,
                Username = user.Username,
                Email = user.Email,
                IsOperator = partner.IsOperator,
                IsAgent = partner.IsAgent,
            };
        }



        //public BaseResponse SaveUser(RegisterCredentialVM register)
        //{

        //    if (register.UserId > 0)
        //    {
        //        var user = _userRepo.Table.Where(x => x.UserId == register.UserId && x.IsDeleted == false).FirstOrDefault();
        //        if (user.UserName != register.UserName)
        //        {
        //            var alreadyExist = _userRepo.Table.Where(x => x.UserName == register.UserName && x.UserId != register.UserId).FirstOrDefault();
        //            if (alreadyExist != null)
        //            {
        //                return new BaseResponse()
        //                {
        //                    Status = HttpStatusCode.OK,
        //                    Message = "User already exist",
        //                };
        //            }
        //        }
        //        var previousResult = AutoMapperHelper.MapSingleRow<User, User>(user);
        //        if (register.DobStr != null)
        //        {
        //            register.Dob = DateTime.Parse(register.DobStr);
        //        }

        //        user.FirstName = register.FirstName;
        //        user.MiddleName = register.MiddleName;
        //        user.LastName = register.LastName;
        //        user.PersonalMobileNumber = register.PersonalMobileNumber;
        //        user.UserName = register.UserName;
        //        user.PrimaryEmail = register.PrimaryEmail;
        //        user.StateKey = register.StateKey;
        //        user.Dob = register.Dob;
        //        user.Gender = register.GenderId.ToString();
        //        user.HomeAddress = register.HomeAddress;
        //        user.City = register.City;
        //        user.Zip = register.Zip;
        //        user.TwoFactorEnabled = register.TwoFactorEnabled;
        //        user.IsActive = register.IsActive;
        //        user.ModifiedBy = register.ModifiedBy;
        //        user.ModifiedDate = DateTime.UtcNow;
        //        user.IsEms = register.IsEMS;
        //        user.IsDiscoveredByOtherOrganization = register.IsDiscoveredByOtherOrganization;

        //        if (!string.IsNullOrEmpty(register.UserImage))
        //        {
        //            string FilePath = "UserProfiles";
        //            register.UserImageByte = Convert.FromBase64String(register.UserImage.Split("base64,")[1]);
        //            string fileName = $"{user.FirstName}-{user.LastName}_{user.UserId}.png";
        //            user.UserImage = $"{FilePath}/{fileName}";
        //            var saveImage = this._communicationService.UploadAttachmentToS3Bucket(register.UserImageByte, FilePath, fileName);
        //        }
        //        _userRepo.Update(user);

        //        var updatedResult = user;
        //        var differences = HelperExtension.GetDifferences(previousResult, updatedResult, ObjectTypeEnums.Model.ToInt());
        //        this._dbContext.Log(differences.updatedRecord, ActivityLogTableEnums.Users.ToString(), user.UserId, ActivityLogActionEnums.Update.ToInt(), differences.previousRecord);


        //        if (user.IsEms)
        //        {
        //            try
        //            {
        //                var codes = this._dbContext.LoadStoredProcedure("md_addEMSUserRole")
        //                                  .WithSqlParam("@UserId", user.UserId)
        //                                  .ExecuteStoredProc<int>();
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //        }
        //        else if (register.IsSuperAdmin)
        //        {
        //            ///////////////// Delete Existing Roles /////////////////
        //            var existingRoles = this._userRoleRepo.Table.Where(x => x.UserIdFk == user.UserId).ToList();
        //            this._userRoleRepo.DeleteRange(existingRoles);
        //            /////////////////////////////////////////////////////////

        //            //////////////// Delete Existing User Relations /////////////////
        //            var existingUserRelation = this._userRelationRepo.Table.Where(x => x.UserIdFk == user.UserId).ToList();
        //            this._userRelationRepo.DeleteRange(existingUserRelation);
        //            /////////////////////////////////////////////////////////////////


        //            /////////////// Add Super Admin Role /////////////////////////////
        //            var superAdminRoleId = this._roleRepo.Table.Where(x => !x.IsDeleted && x.IsSuperAdmin).Select(x => x.RoleId).FirstOrDefault();
        //            var userRole = new UserRole() { UserIdFk = user.UserId, RoleIdFk = superAdminRoleId };
        //            this._userRoleRepo.Insert(userRole);
        //            /////////////////////////////////////////////////////////////////
        //        }
        //        else
        //        {
        //            var roleIds = register.RoleIds != null && register.RoleIds != "" && register.RoleIds != "0" ? register.RoleIds.ToIntList() : new List<int>();
        //            if (roleIds.Count > 0)
        //            {
        //                var alreadyExistRoles = this._userRoleRepo.Table.Where(x => x.UserIdFk == user.UserId).ToList();
        //                if (alreadyExistRoles.Count > 0)
        //                {
        //                    this._userRoleRepo.DeleteRange(alreadyExistRoles);
        //                }
        //            }
        //            List<UserRole> userRoleList = new List<UserRole>();
        //            foreach (var item in roleIds)
        //            {
        //                userRoleList.Add(new UserRole() { UserIdFk = user.UserId, RoleIdFk = item });
        //            }
        //            if (userRoleList.Count > 0)
        //                _userRoleRepo.Insert(userRoleList);
        //        }

        //        return new BaseResponse()
        //        {
        //            Status = HttpStatusCode.OK,
        //            Message = "User updated",
        //            Body = user.UserId,
        //        };
        //    }
        //    else
        //    {
        //        //first validate the username and password from the db and then generate token
        //        if (!string.IsNullOrEmpty(register.UserName) && !string.IsNullOrEmpty(register.Password)
        //            && !string.IsNullOrEmpty(register.PrimaryEmail) && !string.IsNullOrEmpty(register.GenderId.ToString()))
        //        {
        //            var alreadyExist = _userRepo.GetList().Where(x => x.UserName == register.UserName && x.PrimaryEmail == register.PrimaryEmail && x.IsDeleted == false).FirstOrDefault();
        //            if (alreadyExist == null)
        //            {
        //                var randomString = HelperExtension.CreateRandomString();
        //                while (_userRepo.Table.Count(u => u.UserUniqueId == randomString && !u.IsDeleted) > 0)
        //                {
        //                    randomString = HelperExtension.CreateRandomString();
        //                }

        //                bool isMatched = true;
        //                bool from2ndName = true;
        //                int numOfLetters = 1;
        //                while (isMatched)
        //                {
        //                    if (_userRepo.Table.Any(x => x.Initials == register.Initials && !x.IsDeleted))
        //                    {
        //                        numOfLetters++;
        //                        if (numOfLetters <= (register.FirstName.Length) && numOfLetters <= (register.LastName.Length) && from2ndName)
        //                        {
        //                            register.Initials = (register.FirstName.Substring(0, numOfLetters - 1) + register.LastName.Substring(0, numOfLetters)).ToUpper();
        //                        }
        //                        if (numOfLetters <= (register.FirstName.Length) && numOfLetters <= (register.LastName.Length) && !from2ndName)
        //                        {
        //                            register.Initials = (register.FirstName.Substring(0, numOfLetters) + register.LastName.Substring(0, numOfLetters - 1)).ToUpper();
        //                        }
        //                        if (numOfLetters > (register.FirstName.Length) && numOfLetters > (register.LastName.Length))
        //                        {
        //                            numOfLetters = 1;
        //                            from2ndName = !from2ndName;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        isMatched = false;
        //                    }
        //                }


        //                var notificationChannel = this._communicationService.createConversationChannel(register.FirstName + " " + register.LastName, randomString, "");
        //                var conversationUser = this._communicationService.createConversationUser(randomString, register.FirstName + " " + register.LastName);
        //                var addConversationUserToChannel = this._communicationService.addNewUserToConversationChannel(notificationChannel.Sid, randomString);
        //                if (register.DobStr != null)
        //                {
        //                    register.Dob = DateTime.Parse(register.DobStr);
        //                }
        //                var obj = new User()
        //                {
        //                    FirstName = register.FirstName,
        //                    MiddleName = register.MiddleName,
        //                    LastName = register.LastName,
        //                    Initials = register.Initials,
        //                    UserName = register.UserName,
        //                    Password = register.Password,
        //                    PrimaryEmail = register.PrimaryEmail,
        //                    PersonalMobileNumber = register.PersonalMobileNumber,
        //                    Gender = register.GenderId.ToString(),
        //                    Dob = register.Dob,
        //                    HomeAddress = register.HomeAddress,
        //                    City = register.City,
        //                    Zip = register.Zip,
        //                    TwoFactorEnabled = register.TwoFactorEnabled,
        //                    IsActive = register.IsActive,
        //                    StateKey = register.StateKey,
        //                    CreatedBy = register.CreatedBy,
        //                    CreatedDate = DateTime.UtcNow,
        //                    UserUniqueId = randomString,
        //                    UserChannelSid = notificationChannel.Sid,
        //                    ConversationUserSid = conversationUser.Sid,
        //                    IsDeleted = false,
        //                    IsRequirePasswordReset = true,
        //                    IsEms = register.IsEMS,
        //                    IsDiscoveredByOtherOrganization = register.IsDiscoveredByOtherOrganization
        //                };
        //                _userRepo.Insert(obj);
        //                this._dbContext.Log(new { Name = $"{obj.FirstName} {obj.LastName}" }, ActivityLogTableEnums.Users.ToString(), obj.UserId, ActivityLogActionEnums.Create.ToInt());
        //                if (register.IsEMS)
        //                {
        //                    try
        //                    {
        //                        var codes = this._dbContext.LoadStoredProcedure("md_addEMSUserRole")
        //                                          .WithSqlParam("@UserId", obj.UserId)
        //                                          .ExecuteStoredProc<int>();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                    }

        //                }
        //                else if (register.IsSuperAdmin)
        //                {
        //                    var superAdminRoleId = this._roleRepo.Table.Where(x => !x.IsDeleted && x.IsSuperAdmin).Select(x => x.RoleId).FirstOrDefault();
        //                    var userRole = new UserRole() { UserIdFk = obj.UserId, RoleIdFk = superAdminRoleId };
        //                    this._userRoleRepo.Insert(userRole);
        //                }
        //                else
        //                {
        //                    var roleIds = register.RoleIds.ToIntList();
        //                    List<UserRole> userRoleList = new List<UserRole>();
        //                    foreach (var item in roleIds)
        //                    {
        //                        userRoleList.Add(new UserRole() { UserIdFk = obj.UserId, RoleIdFk = item });
        //                    }
        //                    _userRoleRepo.Insert(userRoleList);
        //                }

        //                if (!string.IsNullOrEmpty(register.UserImage))
        //                {
        //                    var RootPath = this._RootPath; //this._environment.WebRootPath;
        //                    string FilePath = "UserProfiles";
        //                    var targetPath = Path.Combine(RootPath, FilePath);
        //                    if (!Directory.Exists(targetPath))
        //                    {
        //                        Directory.CreateDirectory(targetPath);
        //                    }
        //                    targetPath += "/" + $"{obj.FirstName}-{obj.LastName}_{obj.UserId}.png";
        //                    register.UserImageByte = Convert.FromBase64String(register.UserImage.Split("base64,")[1]);
        //                    using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
        //                    {
        //                        fs.Write(register.UserImageByte);
        //                    }
        //                    var newAddedUser = _userRepo.Table.Where(x => x.UserId == obj.UserId).FirstOrDefault();
        //                    newAddedUser.UserImage = targetPath.Replace(RootPath, "").Replace("\\", "/");
        //                    _userRepo.Update(newAddedUser);
        //                }

        //                string sub = "Account Created.";
        //                string mailBody = $"<b>Hi! {obj.FirstName}, </b><br />" +
        //                    $"<p>Your account is created.</p></br />" +
        //                    $"<p>Thanks!</p>";

        //                this._communicationService.SendEmail(obj.PrimaryEmail, sub, mailBody, null);
        //                return new BaseResponse()
        //                {
        //                    Status = HttpStatusCode.OK,
        //                    Message = "User Created",
        //                    Body = obj.UserId,
        //                };

        //                //var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
        //                //                       .WithSqlParam("@componentName", "Users")
        //                //                       .ExecuteStoredProc<RegisterCredentialVM>(); //.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();

        //                //var notification = new PushNotificationVM()
        //                //{
        //                //    Id = row.CodeStrokeId,
        //                //    OrgId = row.OrganizationIdFk,
        //                //    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
        //                //    From = AuthorEnums.Stroke.ToString(),
        //                //    Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS" : "Inhouse") + " Code Stroke From is Changed",
        //                //    RouteLink1 = "/Home/Activate%20Code/code-strok-form",
        //                //};

        //                //_communication.pushNotification(notification);

        //            }
        //            else
        //            {
        //                return new BaseResponse()
        //                {
        //                    Status = HttpStatusCode.OK,
        //                    Message = "User already exist",
        //                };
        //            }
        //        }
        //    }
        //    return null;
        //}


        #region Reset Password

        //public BaseResponse ChangePassword(ChangePasswordVM changePassword)
        //{
        //    DateTime? passwordExpiryDate = null;
        //    var OrgSettings = this._dbContext.LoadStoredProcedure("md_getOrganizationSettings")
        //                        .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
        //                        .WithSqlParam("@IsEMS", ApplicationSettings.isEMS)
        //                        .WithSqlParam("@orgId", changePassword.OrganizationId)
        //                        .ExecuteStoredProc<Setting>().FirstOrDefault();
        //    var user = _userRepo.Table.Where(x => !x.IsDeleted && x.UserId == changePassword.UserId).FirstOrDefault();

        //    if (OrgSettings != null && OrgSettings.EnablePasswordAge.HasValue)
        //    {
        //        passwordExpiryDate = DateTime.UtcNow.AddDays(OrgSettings.EnablePasswordAge.Value);
        //    }
        //    if (changePassword.isFromProfile)
        //    {
        //        var userOldPass = Encryption.decryptData(user.Password, this._encryptionKey);
        //        var oldPass = Encryption.decryptData(changePassword.OldPassword, this._encryptionKey);
        //        var newpasscode = Encryption.decryptData(changePassword.NewPassword, this._encryptionKey);
        //        if (userOldPass == oldPass)
        //        {
        //            if (oldPass == newpasscode)
        //            {
        //                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Old password and new password could not be same. Please update with new Password." };
        //            }
        //            //user.TwoFactorEnabled = OrgSettings.TwoFactorEnable;
        //            //user.TwoFactorExpiryDate = OrgSettings.TwoFactorAuthenticationExpiryMinutes > 0 ? DateTime.UtcNow.AddMinutes(OrgSettings.TwoFactorAuthenticationExpiryMinutes) : user.TwoFactorExpiryDate;
        //            user.PasswordExpiryDate = passwordExpiryDate;
        //            user.Password = changePassword.NewPassword;
        //            user.ModifiedBy = ApplicationSettings.UserId;
        //            user.ModifiedDate = DateTime.UtcNow;
        //            _userRepo.Update(user);
        //            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Password Change Successfully" };
        //        }
        //        else
        //        {
        //            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Old Password is not correct. Please write correct old password" };
        //        }
        //    }
        //    else
        //    {
        //        //user.TwoFactorEnabled = OrgSettings.TwoFactorEnable;
        //        //user.TwoFactorExpiryDate = OrgSettings.TwoFactorAuthenticationExpiryMinutes > 0 ? DateTime.UtcNow.AddMinutes(OrgSettings.TwoFactorAuthenticationExpiryMinutes) : user.TwoFactorExpiryDate;
        //        user.PasswordExpiryDate = passwordExpiryDate;
        //        user.IsRequirePasswordReset = true;
        //        user.Password = changePassword.NewPassword;
        //        user.ModifiedBy = ApplicationSettings.UserId;
        //        user.ModifiedDate = DateTime.UtcNow;
        //        _userRepo.Update(user);
        //        return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Password Change Successfully" };
        //    }
        //}

        //public string SendResetPasswordMail(string email, string url)
        //{
        //    try
        //    {
        //        var user = _userRepo.Table.Where(x => x.PrimaryEmail == email).FirstOrDefault();
        //        if (user != null)
        //        {
        //            string Name = string.Empty;
        //            string MrOrMrs = "Mr/Mrs.";
        //            if (!string.IsNullOrEmpty(user.FirstName))
        //            {
        //                Name = user.FirstName;
        //                if (!string.IsNullOrEmpty(user.LastName))
        //                {
        //                    Name += user.LastName;
        //                }
        //            }
        //            else
        //            {
        //                Name = new MailAddress(email).User;
        //            }
        //            if (!string.IsNullOrEmpty(user.Gender))
        //            {
        //                if (user.Gender.Equals("Male"))
        //                {
        //                    MrOrMrs = "Mr.";
        //                }
        //                else if (user.Gender.Equals("Female"))
        //                {
        //                    MrOrMrs = "Mrs.";
        //                }
        //            }
        //            //string siteUrl = _config["siteUrl"];
        //            string hashUserName = Encryption.encryptData(user.UserName, this._encryptionKey);
        //            string mailMessageTemplate = $"<b>Hi! {MrOrMrs} {Name},</b> <br />" +
        //                $"<p>Please <a href='{url + "/" + hashUserName}' target='_blank'>Click here</a> to reset your password.</p> <br />" +
        //                $"<p>If you didn’t ask to reset your password, you can ignore this email.</p> <br /><br />" +
        //                $"<p>Thank You!</p>";
        //            this._communicationService.SendEmail(user.PrimaryEmail, "Reset Password", mailMessageTemplate, null);

        //            return StatusEnums.Success.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //ElmahExtensions.RiseError(ex);
        //        return ex.ToString();
        //    }
        //    return null;
        //}

        //public string ResetPassword(UserCredentialVM credential)
        //{
        //    try
        //    {
        //        var user = _userRepo.Table.Where(x => x.UserName == credential.username).FirstOrDefault();
        //        if (user != null)
        //        {
        //            //var hashPswd = HelperExtension.Encrypt(credential.password);
        //            //var hashPswd = Encryption.encryptData(credential.password, this._encryptionKey);
        //            user.ModifiedBy = user.UserId;
        //            user.ModifiedDate = DateTime.UtcNow;
        //            user.Password = credential.password;
        //            _userRepo.Update(user);
        //            string desc = $"{user.FirstName} {user.LastName} update password from forget password";
        //            this._dbContext.Log(user.getChangedPropertyObject<User>("Password"), ActivityLogTableEnums.Users.ToString(), user.UserId, ActivityLogActionEnums.Update.ToInt(), null, desc);
        //            return StatusEnums.Success.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //ElmahExtensions.RiseError(ex);
        //        return ex.ToString();
        //    }

        //    return null;
        //}

        #endregion

        #region Two Factor Atuthentication
        //public BaseResponse TwoFactorAuthentication(RequestTwoFactorAuthenticationCode Authentication)
        //{
        //    var user = _userRepo.Table.Where(u => u.UserId == Authentication.UserId).FirstOrDefault();

        //    var Authentication_Code_Sent = Send_Two_Factor_Authentication_Code(user, Authentication);
        //    VerifyTwoFactorAuthenticationCode responseBody;
        //    string ResponseMessage = "";
        //    if (Authentication_Code_Sent)
        //    {
        //        ResponseMessage = "Authentication Code Sent";
        //        responseBody = new VerifyTwoFactorAuthenticationCode
        //        {
        //            UserId = user.UserId,
        //            AuthenticationCode = user.TwoFactorCode,
        //            AuthenticationCodeExpireTime = user.CodeExpiryTime,
        //            AuthenticationCodeExpiresInMinutes = _config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt()
        //        };
        //    }
        //    else
        //    {
        //        ResponseMessage = "Authentication Code Not Sent";
        //        responseBody = null;
        //    }
        //    return new BaseResponse
        //    {
        //        Status = HttpStatusCode.OK,
        //        Message = ResponseMessage,
        //        Body = responseBody
        //    };
        //}
        //public bool Send_Two_Factor_Authentication_Code(User user, RequestTwoFactorAuthenticationCode Authentication)
        //{
        //    try
        //    {
        //        string Two_Factor_Authentication_Code = GenerateTwoFactorAuthenticationCode();
        //        string Message_Body = "MD Route Two Factor Authentication Code: " + Two_Factor_Authentication_Code;
        //        bool Code_Sent = false;
        //        if (Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.S.ToString()))
        //        {
        //            Code_Sent = this._communicationService.SendSms(user.PersonalMobileNumber, Message_Body);
        //        }
        //        else if (Authentication.SendCodeOn.Equals(TwoFactorAuthenticationEnums.E.ToString()))
        //        {
        //            Code_Sent = this._communicationService.SendEmail(user.PrimaryEmail, "Authentication Code", Message_Body, null);
        //        }

        //        if (Code_Sent)
        //        {
        //            user.TwoFactorCode = Two_Factor_Authentication_Code;
        //            user.CodeExpiryTime = DateTime.UtcNow.AddMinutes(_config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt());
        //            _userRepo.Update(user);
        //        }



        //        return Code_Sent;

        //    }
        //    catch (Exception ex)
        //    {
        //        //ElmahExtensions.RiseError(ex);
        //        return false;

        //    }
        //}
        //public string GenerateTwoFactorAuthenticationCode()
        //{
        //    string chars = _config["TwoFactorAuthentication:TwoFactorAuthenticationCode"].ToString();
        //    var random = new Random();
        //    return new string(
        //    Enumerable.Repeat(chars, 6)
        //    .Select(s => s[random.Next(s.Length)])
        //    .ToArray());
        //}

        //public BaseResponse VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationCode verifyCode)
        //{
        //    var user = _userRepo.Table.Where(u => u.UserId == verifyCode.UserId && !u.IsDeleted).FirstOrDefault();
        //    if (user != null)
        //    {
        //        if (verifyCode.AuthenticationCode == user.TwoFactorCode)
        //        {
        //            if (verifyCode.isVerifyForFuture)
        //            {
        //                user.IsTwoFactRememberChecked = true;
        //                user.TwoFactorExpiryDate = DateTime.UtcNow.AddDays(_config["TwoFactorAuthentication:VerifyForFutureDays"].ToInt());
        //                _userRepo.Update(user);
        //            }
        //            verifyCode.AuthenticationStatus = "Verified";
        //            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Authentication Code Verified.", Body = verifyCode };
        //        }
        //        else
        //        {
        //            verifyCode.AuthenticationStatus = "Not Verified";
        //            return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "Authentication Code Mismatch.", Body = verifyCode };
        //        }

        //    }
        //    else
        //    {

        //        return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
        //    }
        //}

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Web.API.Helper;
using Web.Data.Interfaces;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AuthService : IJwtAuthService
    {
        //private readonly IUserAuthRepository _userAuthRepository;
        private readonly GenericRepository<User> _userRepo;
        private readonly GenericRepository<UserRole> _userRoleRepo;
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        public AuthService(IConfiguration config, /*IUserAuthRepository userAuthRepository,*/ IRepository<User> userRepo, IRepository<UserRole> userRoleRepo)
        {
            _config = config;
            //_userAuthRepository = userAuthRepository;
            _userRepo = (GenericRepository<User>)userRepo;
            _userRoleRepo = (GenericRepository<UserRole>)userRoleRepo;
        }


        public BaseResponse Authentication(UserCredential login)
        {
            BaseResponse response = new BaseResponse();
            if (!string.IsNullOrEmpty(login.email) && !string.IsNullOrEmpty(login.password))
            {
                login.password = HelperExtension.Encrypt(login.password);
                var result = _userRepo.Table.Where(x => x.PrimaryEmail == login.email && x.Password == login.password).FirstOrDefault();
                if (result != null)
                {
                    var AuthorizedUser = GenerateJSONWebToken(result);
                    response.Data = AuthorizedUser; ;
                    response.Success = HttpStatusCode.OK;
                    response.Message = "User found";
                }
                else
                {
                    response.Data = null;
                    response.Success = HttpStatusCode.NotFound;
                    response.Message = "User not found";
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
            var tokenExpiryTime = DateTime.Now.AddMinutes(120);
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
                PhoneNumber = user.PersonalMobileNumber

            };
        }

        public string SaveUser(RegisterCredential register)
        {
            if (register.UserId > 0)
            {
                var user = AutoMapperHelper.MapSingleRow<RegisterCredential, User>(register);
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

                        var roleIds = register.RoleIds.Split(',').Select(int.Parse).ToList();
                        List<UserRole> userRoleList = new List<UserRole>();
                        foreach (var item in roleIds)
                        {
                            userRoleList.Add(new UserRole() { UserId = obj.UserId, RoleId = item });
                        }
                        _userRoleRepo.Insert(userRoleList);

                        if (register.UserImage != null && register.UserImage.Count() > 0)
                        {
                            var outPath = Directory.GetCurrentDirectory(); //_config["FilePath:Path"];
                                                                           //if (!Directory.Exists(outPath))
                                                                           //{
                                                                           //    Directory.CreateDirectory(outPath);
                                                                           //}
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
    }
}

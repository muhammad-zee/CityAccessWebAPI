using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Services.Interfaces;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Data.Models;
using Web.Services.CommonVM;
using Web.Model.Common;
using System.Net;
using Web.Services.Helper;

namespace Web.Services.Concrete
{
    public class UsersService : IUsersService

    {
        private readonly IGenericRepository<User> _usersRepo;

        public UsersService(IGenericRepository<User> usersRepo)
        {
            this._usersRepo = usersRepo;
        }
        public BaseResponse GetUserDetails(int UserId)
        {
            var user = this._usersRepo.Table.Where(x => x.Id == UserId && x.IsActive != false).FirstOrDefault();
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Data returned", Body = user };
        }
        public BaseResponse SaveUser(UserVM user)
        {
            if (user.Id > 0)
            {
                var dbUser = this._usersRepo.Table.Where(x => x.Id == user.Id && x.IsActive != false).FirstOrDefault();
                if (dbUser.Password == user.ConfirmPassword)
                {

                    dbUser.Username = user.UserName;
                    dbUser.FullName = user.FullName;
                    dbUser.Email = user.Email;
                    dbUser.Phone = user.Phone;
                    this._usersRepo.Update(dbUser);
                    return new BaseResponse { Status = HttpStatusCode.OK, Message = "User's data updated successfully" };
                }
                else
                {
                    return new BaseResponse { Status = HttpStatusCode.BadRequest, Message = "Password incorrect" };

                }
            }
            else
            {
                User newUser = new User
                {

                    UserIcalLink = Guid.NewGuid(),
                    Username = user.UserName,
                    FullName = user.FullName,
                    Password = user.NewPassword,
                    Email = user.Email,
                    Phone = user.Phone,
                    //PartnerId = user.
                    IsActive = true,
                    EmailConfirmed = false,
                    IsAdmin = true
                };
                this._usersRepo.Insert(newUser);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User created successfully" };
            }
        }

        public BaseResponse UpdatePassword(UserVM user)
        {
            var dbUser = this._usersRepo.Table.Where(x => x.Id == user.Id && x.IsActive != false).FirstOrDefault();
            user.OldPassword = Encryption.MD5Hash(user.OldPassword);
            if (dbUser.Password == user.OldPassword)
            {
                dbUser.Password = Encryption.MD5Hash(user.NewPassword);
                this._usersRepo.Update(dbUser);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Password changed successfully" };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.BadRequest, Message = "Password incorrect" };
            }
        }
        public BaseResponse CheckIfUsernameAvailable(string Username)
        {
            var usernameCount= this._usersRepo.Table.Count(x => x.Username == Username);
            if (usernameCount > 0)
            {
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Username already exists",Body=new { usernameAvailable=false,message= "Username already exists" } };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Username available", Body = new { usernameAvailable = true, message = "Username available" } };

            }
        }

    }
}

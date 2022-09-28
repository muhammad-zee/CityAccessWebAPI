using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Services.Interfaces;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Data.Models;
using Web.Model.Common;
using System.Net;
using Web.Services.Helper;

namespace Web.Services.Concrete
{
    public class UsersService : IUsersService
    {
        private readonly IGenericRepository<User> _usersRepo;
        private readonly IGenericRepository<Partner> _partnerRepo;

        public UsersService(IGenericRepository<User> usersRepo, IGenericRepository<Partner> partnerRepo)
        {
            this._usersRepo = usersRepo;
            this._partnerRepo = partnerRepo;
        }
        public UserDetailVM GetUserDetails(int UserId)
        {
            var user = this._usersRepo.Table.FirstOrDefault(x => x.Id == UserId && x.IsActive == true);
            var userPartner = this._partnerRepo.Table.FirstOrDefault(p => p.Id == user.PartnerId);
            var response = new UserDetailVM
            {
                UserId = user.Id,
                UserName = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                PartnerId = user.PartnerId,
                PartnerTradeName = userPartner.TradeName,
                LastLoginDate = user.LastLoginDate,
                IsAdmin = true,
            };
            return response;
        }
        public BaseResponse SaveUser(UserVM user)
        {
            if (user.UserId > 0)
            {
                var dbUser = this._usersRepo.Table.Where(x => x.Id == user.UserId && x.IsActive != false).FirstOrDefault();
                user.Password = Encryption.MD5Hash(user.Password);
                if (dbUser.Password == user.Password)
                {
                    dbUser.Username = user.UserName;
                    dbUser.FullName = user.FullName;
                    dbUser.Email = user.Email;
                    dbUser.Phone = user.Phone;
                    dbUser.IsAdmin = user.IsAdmin;
                    dbUser.IsActive = user.IsActive;
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
                    Password = Encryption.MD5Hash(user.Password),
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

        public BaseResponse UpdatePassword(UserPasswordVM user)
        {
            var dbUser = this._usersRepo.Table.Where(x => x.Id == user.UserId && x.IsActive != false).FirstOrDefault();
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
        public bool CheckIfUsernameAvailable(string Username)
        {
            var usernameCount = this._usersRepo.Table.Count(x => x.Username == Username);
            return usernameCount == 0;
        }
        public IQueryable<UserDetailVM> GetAllUser()
        {
            var userList = _usersRepo.Table.Where(x => x.IsActive == true);
            var responseList = userList.Select(user => new UserDetailVM
            {
                UserId = user.Id,
                UserName = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                PartnerId = user.PartnerId,
                PartnerTradeName = this._partnerRepo.Table.Where(p => p.Id == user.PartnerId).Select(p => p.TradeName).FirstOrDefault(),
                LastLoginDate = user.LastLoginDate,
                IsAdmin = true,
            }).AsQueryable();
            return responseList;
        }

    }
}

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
        public BaseResponse UpdateUser(UserVM user)
        {
            var dbUser = this._usersRepo.Table.Where(x => x.Id == user.Id && x.IsActive != false).FirstOrDefault();
            if(dbUser.Password == user.ConfirmPassword)
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
        public BaseResponse UpdatePassword(UserVM password)
        {
            var dbUser = this._usersRepo.Table.Where(x => x.Id == password.Id && x.IsActive != false).FirstOrDefault();
            if (dbUser.Password == password.OldPassword)
            {
                dbUser.Password = password.ConfirmPassword;
                this._usersRepo.Update(dbUser);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Password change successfully" };
             }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.BadRequest, Message = "Password not same" };
            }
        }

    }
}

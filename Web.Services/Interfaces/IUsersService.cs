using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IUsersService
    {
        UserDetailVM GetUserDetails(int UserId);
        BaseResponse SaveUser(UserVM user);
        BaseResponse UpdatePassword(UserPasswordVM user);
        bool CheckIfUsernameAvailable(string Username);
        IQueryable<UserDetailVM> GetAllUser();
    }
}

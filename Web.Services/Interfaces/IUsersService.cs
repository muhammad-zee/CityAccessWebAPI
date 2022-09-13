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
        BaseResponse GetUserDetails(int UserId);

        BaseResponse UpdateUser(UserVM user);

        BaseResponse UpdatePassword(UserVM password);
    }
}

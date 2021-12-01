using Web.Data.Interfaces;
using Web.Data.Models;
using Web.DLL.Generic_Repository;

namespace Web.Data.Concrete
{
    public class UserAuthRepository : GenericRepository<UserVM>, IUserAuthRepository
    {
        public UserAuthRepository(RAQ_DbContext context)
              : base(context)
        {

        }
    }
}

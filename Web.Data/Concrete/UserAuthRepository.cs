using Web.Data.Interfaces;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.DLL.Models;

namespace Web.Data.Concrete
{
    public class UserAuthRepository : GenericRepository<User>, IUserAuthRepository
	{
		public UserAuthRepository(RAQ_DbContext context)
			  : base(context)
		{

		}
	}
}

using Web.Data.Interfaces;
using Web.DLL;
using Web.DLL.Db_Context;
using Web.DLL.Generic_Repository;
using Web.DLL.Models;

namespace Web.Data.Concrete
{
    public class HRMSUserAuthRepository : GenericRepository<EmsTblHrmsUser>, IHRMSUserAuthRepository
	{
		public HRMSUserAuthRepository(DbHRMSContext context)
			  : base(context)
		{

		}
	}
}

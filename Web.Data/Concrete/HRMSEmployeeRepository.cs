using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Interfaces;
using Web.Data.PartialClasses;
using Web.DLL.Db_Context;
using Web.DLL.Generic_Repository;

namespace Web.Data.Concrete
{
    public class HRMSEmployeeRepository : GenericRepository<EmsTblEmployeeDetails>, IHRMSEmployeeRepository
    {
        public HRMSEmployeeRepository(DbHRMSContext context)
              : base(context)
        {

        }
    }
}

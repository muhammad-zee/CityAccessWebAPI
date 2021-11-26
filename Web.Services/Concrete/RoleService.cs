using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class RoleService :IRoleService
    {
        private IRepository<Role> _role;
            IConfiguration _config;
        public RoleService(IConfiguration config,IRepository<Role> role)
        {
            this._role = role;
            this._config = config;
        }
        public IQueryable<Role> getRoleList()
        {
            return this._role.GetList().Where(item => !item.IsDeleted);
        }
    }
}

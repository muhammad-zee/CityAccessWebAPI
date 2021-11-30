using System.Linq;
using Web.Data.Models;

namespace Web.Services.Interfaces
{
    public interface IRoleService
    {
        IQueryable<Role> getRoleList();

    }
}

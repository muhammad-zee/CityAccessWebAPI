using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
   public interface IServicesService
    {
        BaseResponse GetAllService();
        BaseResponse GetServiceDetails(int ServiceId);
        BaseResponse SaveService(ServicesVM service);
    }
}

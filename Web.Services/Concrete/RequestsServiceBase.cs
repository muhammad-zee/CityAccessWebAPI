using Web.Model;

namespace Web.Services.Concrete
{
    public abstract class RequestsServiceBase
    {
        public abstract BaseResponse GetServices(RequestsFilterVM filter);
    }
}
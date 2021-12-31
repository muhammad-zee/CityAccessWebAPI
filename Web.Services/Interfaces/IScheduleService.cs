using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IScheduleService
    {
        BaseResponse ImportCSV(ImportCSVFileVM fileVM);
    }
}

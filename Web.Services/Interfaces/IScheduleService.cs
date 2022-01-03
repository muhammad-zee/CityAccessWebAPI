using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IScheduleService
    {
        BaseResponse ImportCSV(ImportCSVFileVM fileVM);
        BaseResponse GetScheduleTemplate(int serviceLine);
    }
}

using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IScheduleService
    {

        BaseResponse getSchedule();
        BaseResponse ImportCSV(ImportCSVFileVM fileVM);
        BaseResponse GetScheduleTemplate(int serviceLine);
        BaseResponse SaveSchedule(ScheduleVM schedule);
    }
}

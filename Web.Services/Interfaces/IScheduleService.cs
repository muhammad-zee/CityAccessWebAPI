using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IScheduleService
    {

        BaseResponse getSchedule();
        BaseResponse GetScheduleList(ScheduleVM schedule);
        BaseResponse ImportCSV(ImportCSVFileVM fileVM);
        BaseResponse GetScheduleTemplate(int serviceLine);
        BaseResponse SaveSchedule(ScheduleVM schedule);
        BaseResponse DeleteSchedule(int scheduleId, int userId);
    }
}

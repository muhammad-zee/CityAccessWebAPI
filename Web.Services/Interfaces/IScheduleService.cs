using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IScheduleService
    {

        BaseResponse getSchedule(EditParams param);
        BaseResponse GetScheduleList(ScheduleVM schedule);
        BaseResponse ImportCSV(ImportCSVFileVM fileVM);
        BaseResponse GetScheduleTemplate(int serviceLine, string roleIds);
        BaseResponse SaveSchedule(ScheduleVM schedule);
        BaseResponse AddUpdateUserSchedule(EditParams param);
        BaseResponse DeleteSchedule(int scheduleId, int userId);
    }
}

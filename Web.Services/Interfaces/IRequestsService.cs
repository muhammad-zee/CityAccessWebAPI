using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IRequestsService
    {
        BaseResponse GetRequestsToUs(RequestsFilterVM filter);
        BaseResponse GetRequestsToUsForCalendar(RequestsFilterVM filter);
        BaseResponse GetRequestDetail(int requestId);
        BaseResponse UpdateRequestStatus(int requestId, string stateId);
        BaseResponse SaveBookingRequest(RequestVM req);
        BaseResponse UpdateRequestAssignee(int requestId, string stateID, string operatorNotes, string ResponsibleID);
    }
}

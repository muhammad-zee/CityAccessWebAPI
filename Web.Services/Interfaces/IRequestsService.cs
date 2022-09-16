﻿using System;
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
        BaseResponse UpdateRequestStatus(int requestId, string stateId);
        BaseResponse SaveBookingRequest(RequestVM req);
    }
}

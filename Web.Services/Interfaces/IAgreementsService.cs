﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IAgreementsService
    {
        BaseResponse GetServices();
        BaseResponse GetAgreements(AgreementsFilterVM filter);
        BaseResponse GetAgreementDetailsByAgreementId(int agreementId);
        BaseResponse SaveAgreement(AgreementVM agreement);
    }
}
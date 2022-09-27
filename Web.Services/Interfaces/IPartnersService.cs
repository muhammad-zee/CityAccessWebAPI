using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
   public interface IPartnersService
    {
        BaseResponse InvitePartner(PartnerInvitationVM partner);
        BaseResponse SavePartner(PartnerVM partner);
        BaseResponse GetAllPartner();
        BaseResponse GetPartnerDetails(int PartnerId);
    }
}

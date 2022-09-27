using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Services.Interfaces;
using Web.Services.CommonVM;
using Web.Model.Common;

namespace Web.Services.Concrete
{
    public class PartnersService : IPartnersService
    {
        private readonly CityAccess_DbContext _dbContext;
        private readonly IConfiguration _config;
        public readonly IEmailService _emailService;

        private readonly IGenericRepository<Partner> _partnersRepo;

        public PartnersService(IConfiguration config,
            CityAccess_DbContext dbContext,
            IEmailService emailService,
            IGenericRepository<Partner> partnersRepo)
        {
            this._config = config;
            this._dbContext = dbContext;
            this._emailService = emailService;
            this._partnersRepo = partnersRepo;
        }
        public BaseResponse InvitePartner(PartnerInvitationVM partner)
        {
            int partnerID = ApplicationSettings.PartnerId;
            string tradename = ApplicationSettings.PartnerTradeName;
            string subject = tradename + " invited you to join City Access";
            string content = partner.EmailMessage + "<br/><br> Follow the link below to create your partner account.";

            var verifyUrl = "/Partners/Create?inv=1&part=" + partnerID;
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            this._emailService.Email_to_send(partner.PartnerEmail, verifyUrl, content, subject);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Mail sent", };
        }

        public BaseResponse SavePartner(PartnerVM param)
        {
            BaseResponse response = new BaseResponse();
            if (param.PartnerId == 0)
            {
                Partner pa = new Partner
                {
                    TradeName = param.TradeName,
                    Description = param.Description,
                    ContactPerson = param.ContactPerson,
                    ContactEmail = param.ContactEmail,
                    ContactPhone = param.ContactPhone,
                    Email = param.NotificationEmail,
                    InvoiceName = param.InvoiceName,
                    InvoiceAddress = param.InvoiceAddress,
                    IsAgent = param.IsAgent,
                    IsOperator = param.IsOperator,
                    IsActive = param.IsActive,
                    IsPublic = param.IsPublic,
                    CountryId = param.Country,
                    //  Logo = partner.Logo,
                };
                this._partnersRepo.Insert(pa);
                this.PartnerSuccess(pa);
                response.Status = HttpStatusCode.OK;
                response.Message = "Partner created successfully";
            }
            else
            {
                Partner pa = this._partnersRepo.Table.FirstOrDefault(a => a.Id == param.PartnerId);
                pa.TradeName = param.TradeName;
                pa.Description = param.Description;
                pa.ContactPerson = param.ContactPerson;
                pa.ContactEmail = param.ContactEmail;
                pa.ContactPhone = param.ContactPhone;
                pa.Email = param.NotificationEmail;
                pa.InvoiceName = param.InvoiceName;
                // VatNumber = partner.VatNumber,
                pa.InvoiceAddress = param.InvoiceAddress;
                pa.IsAgent = param.IsAgent;
                pa.IsOperator = param.IsOperator;
                pa.IsActive = param.IsActive;
                pa.IsPublic = param.IsPublic;
                pa.CountryId = param.Country;
                //  Logo = partner.Logo,
                this._partnersRepo.Update(pa);
                response.Status = HttpStatusCode.OK;
                response.Message = "Partner updated successfully";
            }
            return response;
        }
        private void PartnerSuccess(Partner x)
        {

            string subject = "Your City Access partner account creation was successfull.";
            string content = "<br/><br> After your successfull partner registration follow the link below to register your admin user.";

            var verifyUrl = "/Register/Index?inv=" + x.Id;
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            this._emailService.Email_to_send(x.ContactEmail, verifyUrl, content, subject);
        }

        public BaseResponse GetAllPartner()
        {
            var partnerList = _partnersRepo.Table.Where(p => p.IsActive == true).ToList();
            var responseList = partnerList.Select(partner => new PartnerVM
            {
                TradeName = partner.TradeName,
                Description = partner.Description,
                ContactPerson = partner.ContactPerson,
                ContactEmail = partner.ContactEmail,
                ContactPhone = partner.ContactPhone,
                NotificationEmail = partner.Email,
                InvoiceName = partner.InvoiceName,
                InvoiceAddress = partner.InvoiceAddress,
                IsAgent = partner.IsAgent.Value,
                IsOperator = partner.IsOperator.Value,
                IsActive = partner.IsActive.Value,
                IsPublic = partner.IsPublic.Value,
                Country = partner.CountryId,
                VatNumber = ""
                //  Logo = partner.Logo,
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Partner list return", Body = responseList };
        }
        public BaseResponse GetPartnerDetails(int PartnerId)
        {
            var partner = this._partnersRepo.Table.Where(p => p.Id == PartnerId && p.IsActive != false).FirstOrDefault();
            PartnerVM pa = new PartnerVM
            {
                TradeName = partner.TradeName,
                Description = partner.Description,
                ContactPerson = partner.ContactPerson,
                ContactEmail = partner.ContactEmail,
                ContactPhone = partner.ContactPhone,
                NotificationEmail = partner.Email,
                InvoiceName = partner.InvoiceName,
                InvoiceAddress = partner.InvoiceAddress,
                IsAgent = partner.IsAgent.Value,
                IsOperator = partner.IsOperator.Value,
                IsActive = partner.IsActive.Value,
                IsPublic = partner.IsPublic.Value,
                Country = partner.CountryId,
                VatNumber = ""
                //  Logo = partner.Logo,
            };
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Data returned", Body = pa };
        }

    }
}

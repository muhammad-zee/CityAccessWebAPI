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
using Web.Model.Common;
using Web.Services.CommonVM;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AgreementsService : IAgreementsService
    {
        private readonly CityAccess_DbContext _dbContext;
        private readonly IConfiguration _config;
        public readonly IEmailService _emailService;

        private readonly IGenericRepository<Agreement> _agreementsRepo;
        private readonly IGenericRepository<Service> _servicesRepo;
        private readonly IGenericRepository<Partner> _partnersRepo;
        private readonly IGenericRepository<ServiceImage> _serviceImagesRepo;
        private readonly IGenericRepository<PartnerLogo> _partnerLogosRepo;
        private readonly IGenericRepository<DynamicFieldAlternative> _dynamicFieldAlternativeRepo;

        public AgreementsService(IConfiguration config,
            CityAccess_DbContext dbContext,
            IEmailService emailService,
            IGenericRepository<Agreement> agreementsRepo,
            IGenericRepository<Service> servicesRepo,
            IGenericRepository<Partner> partnersRepo,
            IGenericRepository<ServiceImage> serviceImagesRepo,
            IGenericRepository<PartnerLogo> partnerLogosRepo,
            IGenericRepository<DynamicFieldAlternative> dynamicFieldAlternativeRepo,
            IGenericRepository<Request> requestsRepo,
            IGenericRepository<RequestLog> requestLogsRepo)
        {
            this._config = config;
            this._dbContext = dbContext;
            this._emailService = emailService;

            this._agreementsRepo = agreementsRepo;
            this._servicesRepo = servicesRepo;
            this._partnersRepo = partnersRepo;
            this._serviceImagesRepo = serviceImagesRepo;
            this._partnerLogosRepo = partnerLogosRepo;
            this._dynamicFieldAlternativeRepo = dynamicFieldAlternativeRepo;
        }
        public BaseResponse GetServices()
        {
            var Services = (from ag in this._agreementsRepo.Table
                            join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                            where serv.OperatorId == ApplicationSettings.PartnerId || ag.PartnerId == null || ag.PartnerId == ApplicationSettings.PartnerId
                            select serv).Distinct();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = Services };
        }
        public BaseResponse GetAgreements(AgreementsFilterVM filter)
        {
            var agreements = this._dbContext.LoadStoredProcedure("ca_getAgreementsByPartnerId")
                .WithSqlParam("@pPartnerId",ApplicationSettings.PartnerId)
                .WithSqlParam("@pAgentId", filter.AgentId)
                .WithSqlParam("@pOperatorId", filter.OperatorId)
                .WithSqlParam("@pNotConfirmed", filter.NotConfirmed)
                .WithSqlParam("@pServiceId", filter.ServiceId)
                .WithSqlParam("@pSearchString", filter.SearchString)
                .ExecuteStoredProc<AgreementVM>();


            var frequentlyBookedAgreements = this._dbContext.LoadStoredProcedure("ca_getAgreementsByPartnerId")
                .WithSqlParam("@pPartnerId", ApplicationSettings.PartnerId)
                .WithSqlParam("@pSelectFrequentlyBooked", true)
                .ExecuteStoredProc<AgreementVM>();

            var responseBodyObj = new
            {
                AllAgreements = agreements,
                FrequentlyBookedAgreements = frequentlyBookedAgreements
            };
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = responseBodyObj };
        }
        public BaseResponse GetAgreementDetailsByAgreementId(int agreementId)
        {
            Agreement ag = this._agreementsRepo.Table.FirstOrDefault(a => a.Id == agreementId && a.IsActive != false);
            if (ag != null)
            {
                var serv = this._servicesRepo.Table.Where(x => x.Id == ag.ServiceId).FirstOrDefault();
                //var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
                var servImg = this._serviceImagesRepo.Table.Where(j => j.ServiceId == serv.Id && j.SequenceNr == 1).FirstOrDefault();

                Req_User agr = AgreementDetails(ag);
                agr.Agreement = ag;
                agr.serviceImage = servImg;

                //int partnerID = (int)Session["partnerID"];

                //if (ag.partnerID == partnerID)
                //{
                //    ViewBag.Agent = true;
                //}
                agr.isConfirmed = agr.Agreement.IsConfirmed;
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Data returned", Body = agr };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "Data not found" };

            }
        }
        public Req_User AgreementDetails(Agreement ag)
        {
            Service serv = ag.Service;
            DynamicFieldAlternative dFA = new DynamicFieldAlternative();

            Req_User req_User = new Req_User
            {
                Description = ag.Description,
                CommissionValue = ag.CommissionValue,
                AgentInstructions = ag.AgentInstructions,
                ConfirmationText = ag.MessageTemplate,
                CancellationPolicy = ag.CancellationPolicy,
                PriceValue = ag.Price,
                AgentPaymentValue = ag.PaymentAgent,
            };


            if (ag.PriceType == null)
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == serv.PriceType);
                req_User.PriceType = dFA.Label;
            }
            else
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == serv.PriceType);
                req_User.PriceType = dFA.Label;
            }
            if (ag.TypeCommission == null)
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == serv.ComissionType);
                req_User.CommissionType = dFA.Label;
            }
            else
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == ag.TypeCommission);
                req_User.CommissionType = dFA.Label;
            }
            if (ag.PaymentAgentType == null)
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == serv.PaymentAgentType);
                req_User.AgentPaymentType = dFA.Label;
            }
            else
            {
                dFA = this._dynamicFieldAlternativeRepo.Table.FirstOrDefault(x => x.Id == ag.PaymentAgentType);
                req_User.AgentPaymentType = dFA.Label;
            }
            if (ag.Description == null)
            {
                req_User.Description = serv.Description;
            }
            if (ag.CommissionValue == null)
            {
                req_User.CommissionValue = serv.CommissionValue;
            }
            if (ag.PaymentAgent == null)
            {
                req_User.AgentPaymentValue = serv.PaymentAgent;
            }
            if (ag.Price == null)
            {
                req_User.PriceValue = serv.Price;
            }
            if (ag.AgentInstructions == null)
            {
                req_User.AgentInstructions = serv.AgentInstructions;
            }
            if (ag.MessageTemplate == null)
            {
                req_User.ConfirmationText = serv.ConfirmationText;
            }
            if (ag.CancellationPolicy == null)
            {
                req_User.CancellationPolicy = serv.CancellationPolicy;
            }

            return req_User;
        }

        public BaseResponse SaveAgreement(AgreementVM agreement)
        {
            BaseResponse response = new BaseResponse();
            if (agreement.AgreementId == 0)
            {
                Agreement ag = new Agreement
                {
                    PartnerId = agreement.PartnerId,
                    ServiceId = agreement.ServiceId,
                    Label = agreement.Label,
                    Description = agreement.Description,
                    IsActive = agreement.IsActive,
                    MessageTemplate = agreement.MessageTemplate,
                    AgentInstructions = agreement.AgentInstructions,
                    CancellationPolicy = agreement.CancellationPolicy,
                    NeedsApproval = agreement.NeedsApproval,
                    Price = agreement.AgreementPrice,
                    CommissionType = agreement.CommissionType,
                    CommissionValue = agreement.CommissionValue,
                    Override1 = agreement.Override1,
                    PaymentAgent = agreement.PaymentAgent,
                    EmailToCustomer = agreement.EmailToCustomer,
                    PaymentAgentType = agreement.PaymentAgentType,
                    PriceType = agreement.PriceType,
                    TypeCommission = agreement.TypeCommission,
                    IsConfirmed = agreement.IsConfirmed,
                };
                this._agreementsRepo.Insert(ag);

                //sending agreement creation email
                this.SendAgreementCreationEmail(ag);
                response.Status = HttpStatusCode.OK;
                response.Message = "Agreement created successfully";
            }
            else
            {
                Agreement ag = this._agreementsRepo.Table.FirstOrDefault(a => a.Id == agreement.AgreementId);
                ag.PartnerId = agreement.PartnerId;
                ag.Label = agreement.Label;
                ag.Description = agreement.Description;
                ag.IsActive = agreement.IsActive;
                ag.MessageTemplate = agreement.MessageTemplate;
                ag.AgentInstructions = agreement.AgentInstructions;
                ag.CancellationPolicy = agreement.CancellationPolicy;
                ag.NeedsApproval = agreement.NeedsApproval;
                ag.Price = agreement.AgreementPrice;
                ag.CommissionType = agreement.CommissionType;
                ag.CommissionValue = agreement.CommissionValue;
                ag.Override1 = agreement.Override1;
                ag.PaymentAgent = agreement.PaymentAgent;
                ag.EmailToCustomer = agreement.EmailToCustomer;
                ag.PaymentAgentType = agreement.PaymentAgentType;
                ag.PriceType = agreement.PriceType;
                ag.TypeCommission = agreement.TypeCommission;
                ag.IsConfirmed = agreement.IsConfirmed;
                this._agreementsRepo.Update(ag);
                response.Status = HttpStatusCode.OK;
                response.Message = "Agreement updated successfully";
            }

            return response;
        }

        public void SendAgreementCreationEmail(Agreement ag)
        {

            Partner agent = this._partnersRepo.Table.Where(a => a.Id == ag.PartnerId).FirstOrDefault();
            Service serv = this._servicesRepo.Table.Where(x => x.Id == ag.ServiceId).FirstOrDefault();
            Partner operator1 = this._partnersRepo.Table.Where(a => a.Id == serv.OperatorId).FirstOrDefault();

            string time1 = System.DateTime.Now.ToString("HH:mm");
            //var link =  Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

            string url1 = "Agreements/Confirm/" + ag.Id;
            //url1 = link + url1;

            string subject = "New Agreement by " + operator1.TradeName + " needs confirmation";
            string content = "Agreement details:<br/><br>" +
                 "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                + "<tr><th>Field</th><th>Value</th></tr></thead><tbody>" +
                "<tr><td> Agreement name </td><td> " + ag.Label + " </td></tr>" +
                "<tr><td> Service name </td><td> " + serv.Name + " </td></tr>" +
                "<tr><td> Agent </td><td> " + agent.TradeName + " </td></tr>" +
                "<tr><td> Operator </td><td> " + operator1.TradeName + " </td></tr>" +
                "<tr><td> Confirmation text </td><td> " + ag.MessageTemplate + " </td></tr>" +
                "<tr><td> Agent instructions </td><td> " + ag.AgentInstructions + " </td></tr>" +
                "<tr><td> Cancellation policy </td><td> " + ag.CancellationPolicy + " </td></tr>" +
                "<tr><td> Description </td><td> " + ag.Description + " </td></tr>" +
                "<tr><td> Needs approval </td><td> " + ag.NeedsApproval + " </td></tr>" +
                "<tr><td> Is active </td><td> " + ag.IsActive + " </td></tr>" +
                "<tr><td> Price </td><td> " + ag.Price + " </td></tr>" +
                "<tr><td> Agent payment </td><td> " + ag.PaymentAgent + " </td></tr>" +
                "<tr><td> Commission value </td><td> " + ag.CommissionValue + " </td></tr>" +
                "</tbody></table><br/><br>Click the following link to confirm ";


            this._emailService.Email_to_send(agent.Email, url1, content, subject);

        }

    }
}

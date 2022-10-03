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
        public AgreementsListResponseVM GetAgreements(AgreementsFilterVM filter)
        {
            var dynamicFieldAttributes = this._dynamicFieldAlternativeRepo.Table;
            var agreements = this._dbContext.LoadStoredProcedure("ca_getAgreementsByPartnerId")
                .WithSqlParam("@pPartnerId", ApplicationSettings.PartnerId)
                .WithSqlParam("@pAgentId", filter.AgentId)
                .WithSqlParam("@pOperatorId", filter.OperatorId)
                .WithSqlParam("@pNotConfirmed", filter.NotConfirmed)
                .WithSqlParam("@pServiceId", filter.ServiceId)
                .WithSqlParam("@pSearchString", filter.SearchString)
                .ExecuteStoredProc<AgreementVM>();
            agreements.ForEach(x =>
            {
                x.PaymentAgentTypeLabel = dynamicFieldAttributes.Where(a => a.Id == x.PaymentAgentTypeId).Select(a => a.Label).FirstOrDefault();
                x.PriceTypeLabel = dynamicFieldAttributes.Where(a => a.Id == x.PriceTypeId).Select(a => a.Label).FirstOrDefault();

            });

            var frequentlyBookedAgreements = this._dbContext.LoadStoredProcedure("ca_getAgreementsByPartnerId")
                .WithSqlParam("@pPartnerId", ApplicationSettings.PartnerId)
                .WithSqlParam("@pSelectFrequentlyBooked", true)
                .ExecuteStoredProc<AgreementVM>();
            frequentlyBookedAgreements.ForEach(x =>
            {
                x.PaymentAgentTypeLabel = dynamicFieldAttributes.Where(a => a.Id == x.PaymentAgentTypeId).Select(a => a.Label).FirstOrDefault();
                x.PriceTypeLabel = dynamicFieldAttributes.Where(a => a.Id == x.PriceTypeId).Select(a => a.Label).FirstOrDefault();

            });
            var responseBodyObj = new AgreementsListResponseVM
            {
                AllAgreements = agreements,
                FrequentlyBookedAgreements = frequentlyBookedAgreements
            };
            return responseBodyObj;
        }
        public Agreement GetAgreementDetailsByAgreementId(int agreementId)
        {
            Agreement ag = this._agreementsRepo.Table.FirstOrDefault(a => a.Id == agreementId && a.IsActive != false);
            return ag;

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
                    MessageTemplate = agreement.MessageTemplate,
                    AgentInstructions = agreement.AgentInstructions,
                    CancellationPolicy = agreement.CancellationPolicy,
                    NeedsApproval = agreement.NeedsApproval,
                    Price = agreement.AgreementPrice,
                    TypeCommission = agreement.CommissionTypeId,
                    CommissionType = agreement.CommissionTypeId,
                    CommissionValue = agreement.CommissionValue,
                    Override1 = agreement.Override,
                    PaymentAgent = agreement.PaymentAgent,
                    EmailToCustomer = agreement.EmailToCustomer,
                    PaymentAgentType = agreement.PaymentAgentTypeId,
                    PriceType = agreement.PriceTypeId,
                    IsConfirmed = agreement.IsConfirmed,
                    IsActive = true,
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
                ag.ServiceId = agreement.ServiceId;
                ag.Label = agreement.Label;
                ag.Description = agreement.Description;
                ag.MessageTemplate = agreement.MessageTemplate;
                ag.AgentInstructions = agreement.AgentInstructions;
                ag.CancellationPolicy = agreement.CancellationPolicy;
                ag.NeedsApproval = agreement.NeedsApproval;
                ag.Price = agreement.AgreementPrice;
                ag.TypeCommission = agreement.CommissionTypeId;
                ag.CommissionType = agreement.CommissionTypeId;
                ag.CommissionValue = agreement.CommissionValue;
                ag.Override1 = agreement.Override;
                ag.PaymentAgent = agreement.PaymentAgent;
                ag.EmailToCustomer = agreement.EmailToCustomer;
                ag.PaymentAgentType = agreement.PaymentAgentTypeId;
                ag.PriceType = agreement.PriceTypeId;
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

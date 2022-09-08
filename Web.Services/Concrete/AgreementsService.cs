﻿using Microsoft.Extensions.Configuration;
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
        private readonly IGenericRepository<Request> _requestsRepo;
        private readonly IGenericRepository<RequestLog> _requestLogsRepo;
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
            this._requestsRepo = requestsRepo;
            this._requestLogsRepo = requestLogsRepo;
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
            int partnerId = ApplicationSettings.PartnerId;
            Partner partner = this._partnersRepo.Table.Where(p => p.Id == partnerId && p.IsActive != false).FirstOrDefault();
            IQueryable<AgreementVM> queryable = null;
            var agreements = queryable;
            var agreements1 = queryable;
            int? value = null;
            if (partner.IsAgent == true && partner.IsOperator != true)
            {
                if (this._partnerLogosRepo.Table.Where(x => x.PartnerId == partnerId).Any())
                {
                    agreements = (from ag in this._agreementsRepo.Table
                                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                  join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                  join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                  //join partLogo in db.PartnerLogoes on ag.PartnerId equals partLogo.PartnerID
                                  where servImg.SequenceNr == 1
                                  where serv.IsActive == true
                                  where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value)

                                  //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg/*, PartnerLogo = partLogo*/ }).AsQueryable();
                                  select new AgreementVM
                                  {
                                      Id = ag.Id,
                                      Label = ag.Label,
                                      PartnerId = partn.Id,
                                      PartnerTradeName = partn.TradeName,
                                      ServiceId = serv.Id,
                                      ServiceName = serv.Name,
                                      ServiceImage = servImg.Image,
                                      ServicePrice = serv.Price,
                                      Price = ag.Price
                                  }).AsQueryable();

                    agreements1 = (from ag in this._agreementsRepo.Table
                                   join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                   join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                   where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                   where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value)

                                   //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                   select new AgreementVM
                                   {
                                       Id = ag.Id,
                                       Label = ag.Label,
                                       PartnerId = partn.Id,
                                       PartnerTradeName = partn.TradeName,
                                       ServiceId = serv.Id,
                                       ServiceName = serv.Name,
                                       ServicePrice = serv.Price,
                                       Price = ag.Price
                                   }).AsQueryable();

                    agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                }
                else
                {
                    agreements = (from ag in this._agreementsRepo.Table
                                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                  join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                  join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                  where servImg.SequenceNr == 1
                                  where serv.IsActive == true
                                  where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value)

                                  //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg }).AsQueryable();                                  
                                  select new AgreementVM
                                  {
                                      Id = ag.Id,
                                      Label = ag.Label,
                                      PartnerId = partn.Id,
                                      PartnerTradeName = partn.TradeName,
                                      ServiceId = serv.Id,
                                      ServiceName = serv.Name,
                                      ServiceImage = servImg.Image,
                                      ServicePrice = serv.Price,
                                      Price = ag.Price
                                  }).AsQueryable();

                    agreements1 = (from ag in this._agreementsRepo.Table
                                   join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                   join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                   where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                   where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value)

                                   //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                   select new AgreementVM
                                   {
                                       Id = ag.Id,
                                       Label = ag.Label,
                                       PartnerId = partn.Id,
                                       PartnerTradeName = partn.TradeName,
                                       ServiceId = serv.Id,
                                       ServiceName = serv.Name,
                                       //ServiceImage = servImg.Image,
                                       ServicePrice = serv.Price,
                                       Price = ag.Price
                                   }).AsQueryable();

                    agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                }
            }
            else
            {
                if (partner.IsAgent == true && partner.IsOperator == true)
                {
                    if (this._partnerLogosRepo.Table.Where(x => x.PartnerId == partnerId).Any())
                    {
                        agreements = (from ag in this._agreementsRepo.Table
                                      join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                      join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                      join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                      //join partLogo in db.PartnerLogoes on ag.PartnerId equals partLogo.PartnerID
                                      where servImg.SequenceNr == 1
                                      where serv.IsActive == true
                                      where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value) || (serv.OperatorId == partnerId && ag.PartnerId != null && ag.PartnerId != serv.OperatorId)

                                      //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg, /*PartnerLogo = partLogo*/ }).AsQueryable();
                                      select new AgreementVM
                                      {
                                          Id = ag.Id,
                                          Label = ag.Label,
                                          PartnerId = partn.Id,
                                          PartnerTradeName = partn.TradeName,
                                          ServiceId = serv.Id,
                                          ServiceName = serv.Name,
                                          ServiceImage = servImg.Image,
                                          ServicePrice = serv.Price,
                                          Price = ag.Price
                                      }).AsQueryable();

                        agreements1 = (from ag in this._agreementsRepo.Table
                                       join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                       join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                       where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                       where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value) || (serv.OperatorId == partnerId && ag.PartnerId != null && ag.PartnerId != serv.OperatorId)

                                       //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                       select new AgreementVM
                                       {
                                           Id = ag.Id,
                                           Label = ag.Label,
                                           PartnerId = partn.Id,
                                           PartnerTradeName = partn.TradeName,
                                           ServiceId = serv.Id,
                                           ServiceName = serv.Name,
                                           //ServiceImage = servImg.Image,
                                           ServicePrice = serv.Price,
                                           Price = ag.Price
                                       }).AsQueryable();

                        agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                    }
                    else
                    {
                        agreements = (from ag in this._agreementsRepo.Table
                                      join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                      join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                      join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                      where servImg.SequenceNr == 1
                                      where serv.IsActive == true
                                      where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value) || (serv.OperatorId == partnerId && ag.PartnerId != null && ag.PartnerId != serv.OperatorId)

                                      //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg }).AsQueryable();
                                      select new AgreementVM
                                      {
                                          Id = ag.Id,
                                          Label = ag.Label,
                                          PartnerId = partn.Id,
                                          PartnerTradeName = partn.TradeName,
                                          ServiceId = serv.Id,
                                          ServiceName = serv.Name,
                                          ServiceImage = servImg.Image,
                                          ServicePrice = serv.Price,
                                          Price = ag.Price
                                      }).AsQueryable();

                        agreements1 = (from ag in this._agreementsRepo.Table
                                       join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                       join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                       where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                       where ag.PartnerId == partnerId || (value == null ? ag.PartnerId == null : ag.PartnerId == value) || (serv.OperatorId == partnerId && ag.PartnerId != null && ag.PartnerId != serv.OperatorId)

                                       //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                       select new AgreementVM
                                       {
                                           Id = ag.Id,
                                           Label = ag.Label,
                                           PartnerId = partn.Id,
                                           PartnerTradeName = partn.TradeName,
                                           ServiceId = serv.Id,
                                           ServiceName = serv.Name,
                                           //ServiceImage = servImg.Image,
                                           ServicePrice = serv.Price,
                                           Price = ag.Price
                                       }).AsQueryable();
                        agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                    }
                }
            }
            if (partner.IsAgent != true && partner.IsOperator == true)
            {
                if (this._partnerLogosRepo.Table.Where(x => x.PartnerId == partnerId).Any())
                {
                    agreements = (from ag in this._agreementsRepo.Table
                                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                  join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                  join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                  //join partLogo in db.PartnerLogoes on ag.PartnerId equals partLogo.PartnerID
                                  where servImg.SequenceNr == 1
                                  where serv.IsActive == true
                                  where serv.OperatorId == partnerId

                                  //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg/*, PartnerLogo = partLogo */}).AsQueryable();
                                  select new AgreementVM
                                  {
                                      Id = ag.Id,
                                      Label = ag.Label,
                                      PartnerId = partn.Id,
                                      PartnerTradeName = partn.TradeName,
                                      ServiceId = serv.Id,
                                      ServiceName = serv.Name,
                                      ServiceImage = servImg.Image,
                                      ServicePrice = serv.Price,
                                      Price = ag.Price
                                  }).AsQueryable();

                    agreements1 = (from ag in this._agreementsRepo.Table
                                   join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                   join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                   where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                   where serv.OperatorId == partnerId

                                   //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                   select new AgreementVM
                                   {
                                       Id = ag.Id,
                                       Label = ag.Label,
                                       PartnerId = partn.Id,
                                       PartnerTradeName = partn.TradeName,
                                       ServiceId = serv.Id,
                                       ServiceName = serv.Name,
                                       //ServiceImage = servImg.Image,
                                       ServicePrice = serv.Price,
                                       Price = ag.Price
                                   }).AsQueryable();
                    agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                }
                else
                {
                    agreements = (from ag in this._agreementsRepo.Table
                                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                  join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                  join servImg in this._serviceImagesRepo.Table on serv.Id equals servImg.ServiceId
                                  where servImg.SequenceNr == 1
                                  where serv.IsActive == true
                                  where serv.OperatorId == partnerId

                                  //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg }).AsQueryable();
                                  select new AgreementVM
                                  {
                                      Id = ag.Id,
                                      Label = ag.Label,
                                      PartnerId = partn.Id,
                                      PartnerTradeName = partn.TradeName,
                                      ServiceId = serv.Id,
                                      ServiceName = serv.Name,
                                      ServiceImage = servImg.Image,
                                      ServicePrice = serv.Price,
                                      Price = ag.Price
                                  }).AsQueryable();

                    agreements1 = (from ag in this._agreementsRepo.Table
                                   join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                                   join partn in this._partnersRepo.Table on serv.OperatorId equals partn.Id
                                   where serv.IsActive == true && (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().Id == value)
                                   where serv.OperatorId == partnerId

                                   //select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null }).AsQueryable();
                                   select new AgreementVM
                                   {
                                       Id = ag.Id,
                                       Label = ag.Label,
                                       PartnerId = partn.Id,
                                       PartnerTradeName = partn.TradeName,
                                       ServiceId = serv.Id,
                                       ServiceName = serv.Name,
                                       //ServiceImage = servImg.Image,
                                       ServicePrice = serv.Price,
                                       Price = ag.Price
                                   }).AsQueryable();

                    agreements = agreements.AsEnumerable().Union(agreements1.AsEnumerable()).AsQueryable();
                }
            }

            //if (filter.Agent != null && filter.Agent != "")
            //{
            //    agreements = agreements.Where(x => x.Agreement.Partner.TradeName == filter.Agent);
            //}
            //if (filter.Operator1 != null && filter.Operator1 != "")
            //{
            //    agreements = agreements.Where(x => x.Partner.TradeName == filter.Operator1);
            //}
            //if (filter.agr == true)
            //{
            //    agreements = agreements.Where(x => x.Agreement.IsConfirmed == false || x.Agreement.IsConfirmed == null);
            //}
            //else
            //{
            //    agreements = agreements.Where(x => x.Agreement.IsConfirmed == true);
            //}


            //if (!String.IsNullOrEmpty(filter.SearchString))
            //{
            //    agreements = agreements.Where(s => s.Agreement.Label.Contains(filter.SearchString) || s.Agreement.Description.Contains(filter.SearchString));
            //}

            //if (!String.IsNullOrEmpty(filter.Service))
            //{
            //    agreements = agreements.Where(x => x.Agreement.Service.Name == filter.Service);
            //}

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = agreements };
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
            if (agreement.Id == 0)
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
                    Price = agreement.Price,
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
                Agreement ag = this._agreementsRepo.Table.FirstOrDefault(a => a.Id == agreement.Id);
                ag.PartnerId = agreement.PartnerId;
                ag.Label = agreement.Label;
                ag.Description = agreement.Description;
                ag.IsActive = agreement.IsActive;
                ag.MessageTemplate = agreement.MessageTemplate;
                ag.AgentInstructions = agreement.AgentInstructions;
                ag.CancellationPolicy = agreement.CancellationPolicy;
                ag.NeedsApproval = agreement.NeedsApproval;
                ag.Price = agreement.Price;
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

        public BaseResponse SaveAgreementBooking(RequestVM requestParam, bool? FromPartnerSite)
        {
            BaseResponse response = new BaseResponse();
            Request request = new Request
            {
                ContactName = requestParam.ContactName,
                ContactEmail = requestParam.ContactEmail,
                ContactPhone = requestParam.ContactPhone,
                EventDate = requestParam.EventDate,
                EventTime = requestParam.EventTime,
                NrPersons = requestParam.NrPersons,
                Price = requestParam.Price,
                Reference = requestParam.Reference,
                Notes = requestParam.Notes,
                //IsTransfer=requestParam.IsTransfer,
                PickupLocation = requestParam.PickupLocation,
                DropoffLocation = requestParam.DropoffLocation,
                FlightNr = requestParam.FlightNr,
                //HasReturn=requestParam.hasreturn,
                ReturnDate = requestParam.returnDate,
                ReturnTime = requestParam.returnTime,
                ReturnFlight = requestParam.ReturnFlight,
                ReturnPickup = requestParam.ReturnPickup,
                ReturnDropoff = requestParam.ReturnDropoff,
                ExtraDate1 = requestParam.ExtraDate1,
                ExtraDate2 = requestParam.ExtraDate2,
                ExtraTime1 = requestParam.ExtraTime1,
                ExtraText1 = requestParam.ExtraText1,
                ExtraMultiText1 = requestParam.ExtraMultiText1,
                ClientNotes = requestParam.ClientNotes,
                BookDate = requestParam.BookDate,
                BookTime = requestParam.BookTime
            };
            request.AgreementId = requestParam.AgreementID;
            request.BookDate = System.DateTime.Now;

            DateTime midnigth = new DateTime(request.BookDate.Value.Year, request.BookDate.Value.Month, request.BookDate.Value.Day, 0, 0, 0);

            request.BookTime = request.BookDate - midnigth;

            var ag = this._agreementsRepo.Table.Where(z => z.Id == request.AgreementId).FirstOrDefault();
            var serv = this._servicesRepo.Table.Where(p => p.Id == ag.ServiceId).FirstOrDefault();
            var opr = this._partnersRepo.Table.Where(a => a.Id == serv.OperatorId).FirstOrDefault();
            var part = new Partner();
            if (FromPartnerSite == true)
            {
                request.BookerId = 1;
                request.StateId = Constants.StateTransitions.SiteApproval;
            }
            else
            {
                request.BookerId = ApplicationSettings.UserId;
                part = this._partnersRepo.Table.FirstOrDefault(p => p.Id == ApplicationSettings.PartnerId);
                if (ag.NeedsApproval == true)
                {
                    request.StateId = Constants.StateTransitions.Submitted;
                }
                else
                {
                    request.StateId = Constants.StateTransitions.Approved;
                }
            }
            this._requestsRepo.Insert(request);

            Request req = request;
            int requestID = req.Id;

            RequestLog reqLog = new RequestLog();
            reqLog.Date = DateTime.Now;
            reqLog.Time = DateTime.Now.ToString("HH:mm");
            reqLog.RequestId = requestID;
            reqLog.UserId = ApplicationSettings.UserId;
            reqLog.Notes = FromPartnerSite==true? "Booking created by site request!" : "Booking created!";
            this._requestLogsRepo.Insert(reqLog);

            //Set up and forwarding e - mail to operator
            this.SendServiceBookedEmail(req, part, ag, serv, opr,FromPartnerSite);
            response.Status = HttpStatusCode.OK;
            response.Message = "Service booked successfully";
            return response;
        }
        public void SendServiceBookedEmail(Request request,Partner part,Agreement ag, Service serv,Partner opr,bool? FromPartnerSite)
        {

            string og = " " + request.EventDate.ToString("dd-MM-yyyy");
            string date = og.Replace("12:00:00 AM", " ");
            var link = "";//Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");
            string url = "ServicesRequested/Details/" + request.Id;
            url = link + url;
            string subject = string.Empty;
            string content = string.Empty;
            if (FromPartnerSite == null || FromPartnerSite == false)
            {
                if (ag.NeedsApproval == true)
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-" + part.TradeName + "-" + date + "-" + request.NrPersons + "-needs approval";
                }
                else
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-" + part.TradeName + "-" + date + "-" + request.NrPersons + "-approved";
                }

                 content = "New request from " + part.TradeName + ":<br> ";
                if (request.ReturnDate == null)
                {

                    content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                        "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.Label + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.Price + "</td></tr>";
                    if (request.PickupLocation != null)
                    {
                        content = content +
                        "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>";
                    }
                    if (request.DropoffLocation != null)
                    {
                        content = content +
                        "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>";
                    }
                    if (request.FlightNr != null)
                    {
                        content = content +
                        "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>";
                    }
                    content = content +
                        "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                else
                {
                    og = " " + request.ReturnDate;
                    string returnDate = " " + request.ReturnDate?.ToString("dd-MM-yyyy");

                    content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                         + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                         + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                         + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                         + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                         "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                         "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                         "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                         "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                         "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                         "<tr><td>Price</td><td>" + request.Price + "</td></tr>" +
                         "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>" +
                         "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>" +
                         "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>" +
                         "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                         "<tr><td>Return Time</td><td>" + request.ReturnTime + "</td></tr>" +
                         "<tr><td>Return flight number</td><td>" + request.ReturnFlight + "</td></tr>" +
                         "<tr><td>Return pickup</td><td>" + request.ReturnPickup + "</td></tr>" +
                         "<tr><td>Return dropoff</td><td>" + request.ReturnDropoff + "</td></tr>" +
                         "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                         "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                this._emailService.Email_to_send(opr.Email, url, content, subject);
            }
            else
            {
                if (ag.NeedsApproval == true)
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-from client " + request.ContactName + "-" + date + "-" + request.NrPersons + "-needs approval";
                }
                else
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-from client" + request.ContactName + "-" + date + "-" + request.NrPersons + "-approved";
                }

                content = "New request from " + request.ContactName + ":<br> ";

                if (request.ReturnDate == null)
                {

                    content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                        "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.Label + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.Price + "</td></tr>";
                    if (request.PickupLocation != null)
                    {
                        content = content +
                        "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>";
                    }
                    if (request.DropoffLocation != null)
                    {
                        content = content +
                        "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>";
                    }
                    if (request.FlightNr != null)
                    {
                        content = content +
                        "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>";
                    }
                    content = content +
                        "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                else
                {
                    og = " " + request.ReturnDate;
                    string returnDate = " " + request.ReturnDate?.ToString("dd-MM-yyyy");

                    content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                         + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                         + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                         + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                         + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                         "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                         "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                         "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                         "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                         "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                         "<tr><td>Price</td><td>" + request.Price + "</td></tr>" +
                         "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>" +
                         "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>" +
                         "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>" +
                         "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                         "<tr><td>Return Time</td><td>" + request.ReturnTime + "</td></tr>" +
                         "<tr><td>Return flight number</td><td>" + request.ReturnFlight + "</td></tr>" +
                         "<tr><td>Return pickup</td><td>" + request.ReturnPickup + "</td></tr>" +
                         "<tr><td>Return dropoff</td><td>" + request.ReturnDropoff + "</td></tr>" +
                         "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                         "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
            this._emailService.Email_to_send(part.Email, url, content, subject);
            }


        }
    }
}

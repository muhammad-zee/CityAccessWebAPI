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
using Web.Model.Common;
using Web.Services.Helper;

namespace Web.Services.Concrete
{
    public class ServicesService : IServicesService
    {
        private readonly CityAccess_DbContext _dbContext;
        private readonly IConfiguration _config;
        public readonly IEmailService _emailService;

        private readonly IGenericRepository<Service> _servicesRepo;
        private readonly IGenericRepository<Partner> _partnersRepo;
        private readonly IGenericRepository<CommissionType> _commissiontypeRepo;

        public ServicesService(IConfiguration config,
            CityAccess_DbContext dbContext,
            IEmailService emailService,
            IGenericRepository<Service> servicesRepo, IGenericRepository<Partner> partnersRepo,
             IGenericRepository<CommissionType> commissiontypeRepo) 
        {
            this._config = config;
            this._dbContext = dbContext;
            this._emailService = emailService;
            this._servicesRepo = servicesRepo;
            this._partnersRepo = partnersRepo;
            this._commissiontypeRepo = commissiontypeRepo;

        }
        public BaseResponse GetAllService()
        {
            var ServiceList = _servicesRepo.Table.Where(s => s.IsActive == true).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Sevice list return", Body = ServiceList };
        }
        public BaseResponse GetServiceDetails(int ServiceId)
        {
            var service = this._servicesRepo.Table.Where(s => s.Id == ServiceId && s.IsActive != false).FirstOrDefault();
            var partner = this._partnersRepo.Table.FirstOrDefault(p => p.Id == service.OperatorId && p.IsActive == true);
           var commissiontype = this._commissiontypeRepo.Table.Where(c => c.Id == service.ComissionType ).FirstOrDefault();

            var serviceVM = new ServicesVM
            {
                Id = service.Id,
                ServiceName = service.Name,
                Descritpion = service.Description,
                CommissionTypeId = service.ComissionType,
                CommissionTypeName = commissiontype.Label,
                MaxNumberOfPersons = service.MaxPersonNum,
                MinNumberOfPersons = service.MinPersonNum,
                Availability = service.Availability1,
                Price = service.Price,
                TypeofPrice = service.Price,
                OverridePrice = service.Override1,
                AgentPayment = service.PaymentAgent,
                TypeOfAgentPayment = service.PaymentAgent,
                AgentInstructions = service.AgentInstructions,
                ConfirmationText = service.ConfirmationText,
                CancellationPolicy = service.CancellationPolicy,
                IsPublic = service.IsPublic,
                IsActive = service.IsActive,
                //City = service.City,
                PartnerTradeName = partner.TradeName
                
            };
            return new BaseResponse
            {
                Status = HttpStatusCode.OK,
                Message = "Service detail returned",
                Body = serviceVM
            };
        }
        public BaseResponse SaveService(ServicesVM service)
        {
            if (service.Id > 0)
            {
                var dbService = this._servicesRepo.Table.Where(s => s.Id == service.Id && s.IsActive != false).FirstOrDefault();

                dbService.Id = service.Id;
                // dbService.Type = service.Type;
                dbService.Name = service.ServiceName;
                dbService.Description = service.Descritpion;
                dbService.Duration = service.Duration;
                dbService.ComissionType = service.CommissionTypeId;
                dbService.MaxPersonNum = service.MaxNumberOfPersons;
                dbService.MinPersonNum = service.MinNumberOfPersons;
                dbService.Availability1 = service.Availability;
                dbService.Price = service.Price;
                dbService.Override1 = service.OverridePrice;
                dbService.PaymentAgent = service.AgentPayment;
                dbService.AgentInstructions = service.AgentInstructions;
                dbService.ConfirmationText = service.ConfirmationText;
                dbService.CancellationPolicy = service.CancellationPolicy;
                //   dbService.City = service.City;
                dbService.IsPublic = service.IsPublic;
                dbService.IsActive = service.IsActive;
                //  dbService.ServiceImages = service.image;
                this._servicesRepo.Update(dbService);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Services updated successfully" };
            }
            else
            {
                Service newService = new Service
                {
                    //Type = service.Type,
                    Name = service.ServiceName,
                    Description = service.Descritpion,
                    Duration = service.Duration,
                    ComissionType = service.CommissionTypeId,
                    MaxPersonNum = service.MaxNumberOfPersons,
                    MinPersonNum = service.MinNumberOfPersons,
                    Availability1 = service.Availability,
                    Price = service.Price,
                    Override1 = service.OverridePrice,
                    PaymentAgent = service.AgentPayment,
                    AgentInstructions = service.AgentInstructions,
                    ConfirmationText = service.ConfirmationText,
                    CancellationPolicy = service.CancellationPolicy,
                    IsPublic = service.IsPublic,
                    IsActive = service.IsActive
                };
                this._servicesRepo.Insert(newService);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Services created successfully" };
            }
        }
       
    }
}


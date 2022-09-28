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
        private readonly IGenericRepository<DynamicFieldAlternative> _dynamicFieldAlternative;

        public ServicesService(IConfiguration config,
            CityAccess_DbContext dbContext,
            IEmailService emailService,
            IGenericRepository<Service> servicesRepo, IGenericRepository<Partner> partnersRepo,
             IGenericRepository<CommissionType> commissiontypeRepo,
             IGenericRepository<DynamicFieldAlternative> dynamicFieldAlternative)
        {
            this._config = config;
            this._dbContext = dbContext;
            this._emailService = emailService;
            this._servicesRepo = servicesRepo;
            this._partnersRepo = partnersRepo;
            this._commissiontypeRepo = commissiontypeRepo;
            this._dynamicFieldAlternative = dynamicFieldAlternative;
        }
        public IQueryable<ServicesVM> GetAllService()
        {
            var servicesList = this._dbContext.LoadStoredProcedure("ca_getServicesByPartnerId")
               .WithSqlParam("@pPartnerId", ApplicationSettings.PartnerId)
               .ExecuteStoredProc<ServicesVM>();
            servicesList.ForEach(x => x.ServiceImage = "/Images/logo.png");
            return servicesList.AsQueryable();
        }
        public ServicesVM GetServiceDetails(int ServiceId)
        {
            var serviceDetail = this._dbContext.LoadStoredProcedure("ca_getServiceDetailByServiceId")
                .WithSqlParam("@pServiceId", ServiceId)
                .ExecuteStoredProc<ServicesVM>().FirstOrDefault();
            var dynamicFieldsList = this._dynamicFieldAlternative.Table;
            serviceDetail.PriceTypeLabel = dynamicFieldsList.Where(x => x.Id == serviceDetail.PriceTypeId).Select(x => x.Label).FirstOrDefault();
            serviceDetail.PaymentAgentTypeLabel = dynamicFieldsList.Where(x => x.Id == serviceDetail.PaymentAgentTypeId).Select(x => x.Label).FirstOrDefault();
            serviceDetail.AvailabilityLabel = dynamicFieldsList.Where(x => x.Id == serviceDetail.AvailabilityId).Select(x => x.Label).FirstOrDefault();
            serviceDetail.ServiceImage = "/Images/logo.png";
            return serviceDetail;


        }
        public BaseResponse SaveService(ServicesVM service)
        {
            if (service.ServiceId > 0)
            {
                var dbService = this._servicesRepo.Table.Where(s => s.Id == service.ServiceId && s.IsActive != false).FirstOrDefault();

                dbService.Id = service.ServiceId;
                dbService.TypeId = service.ServiceTypeId;
                dbService.Name = service.ServiceName;
                dbService.Description = service.Descritpion;
                dbService.OperatorId = service.PartnerId;
                dbService.Duration = service.Duration;
                dbService.ComissionType = service.CommissionTypeId;
                dbService.CommissionValue = service.CommissionValue;
                dbService.MaxPersonNum = service.MaxNumberOfPersons;
                dbService.MinPersonNum = service.MinNumberOfPersons;
                dbService.Availability1 = service.AvailabilityId;
                dbService.Price = service.Price;
                dbService.PriceType = service.PriceTypeId;
                dbService.Override1 = service.OverridePrice;
                dbService.PaymentAgent = service.PaymentAgentTypeId;
                dbService.PaymentAgentType = service.PaymentAgentTypeId;
                dbService.AgentInstructions = service.AgentInstructions;
                dbService.ConfirmationText = service.ConfirmationText;
                dbService.CancellationPolicy = service.CancellationPolicy;
                dbService.CityId = service.CityId;
                dbService.IsPublic = service.IsPublic;
                //  dbService.ServiceImages = service.image;
                this._servicesRepo.Update(dbService);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Service updated successfully" };
            }
            else
            {
                Service newService = new Service
                {
                    Name = service.ServiceName,
                    TypeId = service.ServiceTypeId,
                    Description = service.Descritpion,
                    OperatorId = service.PartnerId,
                    Duration = service.Duration,
                    ComissionType = service.CommissionTypeId,
                    CommissionValue = service.CommissionValue,
                    MaxPersonNum = service.MaxNumberOfPersons,
                    MinPersonNum = service.MinNumberOfPersons,
                    Availability1 = service.AvailabilityId,
                    Price = service.Price,
                    PriceType = service.PriceTypeId,
                    Override1 = service.OverridePrice,
                    PaymentAgent = service.PaymentAgent,
                    PaymentAgentType = service.PaymentAgentTypeId,
                    AgentInstructions = service.AgentInstructions,
                    ConfirmationText = service.ConfirmationText,
                    CancellationPolicy = service.CancellationPolicy,
                    CityId = service.CityId,
                    IsPublic = service.IsPublic,
                    IsActive = true
                };
                this._servicesRepo.Insert(newService);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Service created successfully" };
            }
        }

    }
}


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AdminService :IAdminService
    {
        private CityAccess_DbContext _dbContext;
        IConfiguration _config;
        private IHostingEnvironment _environment;
        private readonly IGenericRepository<City> _cityRepo;
        private readonly IGenericRepository<CommissionType> _commissionTypeRepo;
        private readonly IGenericRepository<DynamicField> _dynamicFieldRepo;
        private readonly IGenericRepository<DynamicFieldAlternative> _dynamicFieldAlternativeRepo;
        public AdminService(IConfiguration config,
                   CityAccess_DbContext dbContext,
                   IHostingEnvironment environment,
                   IGenericRepository<City> cityRepo,
                   IGenericRepository<CommissionType> commissionTypeRepo,
                   IGenericRepository<DynamicField> dynamicFieldRepo,
                   IGenericRepository<DynamicFieldAlternative> dynamicFieldAlternativeRepo)
        {
            this._dbContext = dbContext;
            this._config = config;
            this._environment = environment;
            this._cityRepo = cityRepo;
            this._commissionTypeRepo = commissionTypeRepo;
            this._dynamicFieldRepo = dynamicFieldRepo;
            this._dynamicFieldAlternativeRepo = dynamicFieldAlternativeRepo;
        }

        public IQueryable<City> GetAllcities()
        {
            return this._cityRepo.Table;
        }
        public IQueryable<CommissionType> GetAllCommissionTypes()
        {
            return this._commissionTypeRepo.Table;
        }
        public IDictionary<string, IQueryable<DynamicFieldAlternativeVM>> GetAllDynamicFields()
        {
            var dynamicFields = this._dynamicFieldRepo.Table;
            var dynamicFieldsAlternative = this._dynamicFieldAlternativeRepo.Table.Where(x => dynamicFields.Any(df => df.Id == x.DynamicfieldId));
            IDictionary<string, IQueryable<DynamicFieldAlternativeVM>> dictionary = new Dictionary<string, IQueryable<DynamicFieldAlternativeVM>>();
            foreach (var field in dynamicFields)
            {
                var attributes= dynamicFieldsAlternative.Where(x=>x.DynamicfieldId == field.Id).Select(x => new DynamicFieldAlternativeVM
                {
                    Id = x.Id,
                    Label = x.Label,
                    DynamicFieldId = x.DynamicfieldId,
                    DynamicFieldName = field.FieldType
                });
                dictionary.Add(field.FieldType, attributes);
            }
          
            return dictionary;
        }

    }
}

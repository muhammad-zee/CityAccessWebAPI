using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class FacilityService : IFacilityService
    {
        private RAQ_DbContext _dbContext;
        private IRepository<ServiceLine> _serviceRepo;
        private IRepository<Department> _departmentRepo;
        private IRepository<Organization> _organizationRepo;
        private IRepository<DepartmentService> _departmentServiceRepo;
        private IRepository<OrganizationDepartment> _organizationDepartmentRepo;

        public FacilityService(RAQ_DbContext dbContext,
            IRepository<ServiceLine> serviceRepo,
            IRepository<Department> departmentRepo,
            IRepository<Organization> organizationRepo,
            IRepository<DepartmentService> departmentServiceRepo,
            IRepository<OrganizationDepartment> organizationDepartmentRepo
            )
        {
            this._dbContext = dbContext;
            this._serviceRepo = serviceRepo;
            this._departmentRepo = departmentRepo;
            this._organizationRepo = organizationRepo;
            this._departmentServiceRepo = departmentServiceRepo;
            this._organizationDepartmentRepo = organizationDepartmentRepo;
        }

        #region Service Line

        public BaseResponse GetAllServiceLines()
        {
            var services = _serviceRepo.Table.Where(x => x.IsDeleted == false).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = services
            };
        }

        public BaseResponse GetServiceLineById(int Id)
        {
            var serviceLine = _serviceRepo.Table.Where(x => x.ServiceId == Id && x.IsDeleted == false).FirstOrDefault();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = serviceLine
            };
        }

        public BaseResponse AddOrUpdateServiceLine(ServiceLineVM serviceLine)
        {
            BaseResponse response = null;
            if (serviceLine.ServiceId > 0)
            {
                serviceLine.ModifiedDate = DateTime.UtcNow;
                var service = AutoMapperHelper.MapSingleRow<ServiceLineVM, ServiceLine>(serviceLine);
                _serviceRepo.Update(service);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = serviceLine };
            }
            else
            {
                serviceLine.CreatedDate = DateTime.UtcNow;
                var service = AutoMapperHelper.MapSingleRow<ServiceLineVM, ServiceLine>(serviceLine);
                _serviceRepo.Insert(service);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = serviceLine };
            }
            return response;
        }


        #endregion

        #region Department 

        public BaseResponse GetAllDepartments()
        {
            var departments = _departmentRepo.Table.Where(x => x.IsDeleted == false).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = departments
            };
        }

        public BaseResponse GetDepartmentById(int Id)
        {
            var department = _departmentRepo.Table.Where(x => x.DepartmentId == Id && x.IsDeleted == false).FirstOrDefault();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = department
            };
        }

        public BaseResponse AddOrUpdateDepartment(DepartmentVM department)
        {
            BaseResponse response = null;

            var dpt = AutoMapperHelper.MapSingleRow<DepartmentVM, Department>(department);
            var serviceIds = department.ServicesIdFks.ToIntList();
            List<DepartmentService> dptServices = new List<DepartmentService>();
            if (department.DepartmentId > 0)
            {
                dpt.ModifiedDate = DateTime.UtcNow;
                _departmentRepo.Update(dpt);
                var alreadyExistServices = _departmentServiceRepo.Table.Where(x => x.DepartmentIdFk == department.DepartmentId).ToList();
                _departmentServiceRepo.DeleteRange(alreadyExistServices);
                foreach (var item in serviceIds)
                {
                    dptServices.Add(new DepartmentService() { ServiceIdFk = item, DepartmentIdFk = dpt.DepartmentId });
                }
                _departmentServiceRepo.Insert(dptServices);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = dpt };
            }
            else
            {
                dpt.CreatedDate = DateTime.UtcNow;
                _departmentRepo.Insert(dpt);

                foreach (var item in serviceIds)
                {
                    dptServices.Add(new DepartmentService() { ServiceIdFk = item, DepartmentIdFk = dpt.DepartmentId });
                }
                _departmentServiceRepo.Insert(dptServices);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = dpt };
            }
            return response;
        }


        #endregion

        #region Organization 

        public BaseResponse GetAllOrganizations()
        {
            var organizations = _organizationRepo.Table.Where(x => x.IsDeleted == false).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = organizations
            };
        }

        public BaseResponse GetOrganizationById(int Id)
        {
            var organization = _organizationRepo.Table.Where(x => x.OrganizationId == Id && x.IsDeleted == false).FirstOrDefault();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = organization
            };
        }

        public BaseResponse AddOrUpdateOrganization(OrganizationVM organization)
        {
            BaseResponse response = null;
            var org = AutoMapperHelper.MapSingleRow<OrganizationVM, Organization>(organization);
            var dptIds = organization.DepartmentIdsFk.ToIntList();
            List<OrganizationDepartment> orgDpt = new List<OrganizationDepartment>();
            if (organization.OrganizationId > 0)
            {
                org.ModifiedDate = DateTime.UtcNow;
                _organizationRepo.Update(org);
                var alreadyExistDpts = _organizationDepartmentRepo.Table.Where(x => x.OrganizationIdFk == organization.OrganizationId).ToList();
                _organizationDepartmentRepo.DeleteRange(alreadyExistDpts);
                foreach (var item in dptIds)
                {
                    orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                }
                _organizationDepartmentRepo.Insert(orgDpt);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = organization };
            }
            else
            {
                org.CreatedDate = DateTime.UtcNow;
                _organizationRepo.Insert(org);
                foreach (var item in dptIds)
                {
                    orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                }
                _organizationDepartmentRepo.Insert(orgDpt);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = organization };
            }
            return response;
        }


        #endregion

    }
}

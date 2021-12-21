using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
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
        private IRepository<ClinicalHour> _clinicalHour;
        private IRepository<DepartmentService> _departmentServiceRepo;
        private IRepository<OrganizationDepartment> _organizationDepartmentRepo;
        private IRepository<ControlListDetail> _controlListDetails;

        public FacilityService(RAQ_DbContext dbContext,
            IRepository<ServiceLine> serviceRepo,
            IRepository<Department> departmentRepo,
            IRepository<Organization> organizationRepo,
            IRepository<ClinicalHour> clinicalHour,
            IRepository<DepartmentService> departmentServiceRepo,
            IRepository<OrganizationDepartment> organizationDepartmentRepo,
            IRepository<ControlListDetail> controlListDetails
            )
        {
            this._dbContext = dbContext;
            this._serviceRepo = serviceRepo;
            this._departmentRepo = departmentRepo;
            this._organizationRepo = organizationRepo;
            this._clinicalHour = clinicalHour;
            this._departmentServiceRepo = departmentServiceRepo;
            this._organizationDepartmentRepo = organizationDepartmentRepo;
            this._controlListDetails = controlListDetails;
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

        public BaseResponse GetServicesByIds(string Ids)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                var idsList = Ids.ToIntList();
                var services = (from ds in _departmentServiceRepo.Table
                                join s in _serviceRepo.Table on ds.ServiceIdFk equals s.ServiceId
                                where idsList.Contains(ds.DepartmentIdFk) && s.IsDeleted != true
                                select new ServiceLineVM()
                                {
                                    ServiceId = s.ServiceId,
                                    ServiceName = s.ServiceName,
                                    DepartmentIdFk = ds.DepartmentIdFk
                                }).DistinctBy(x => x.ServiceName).ToList();

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = services };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Select at least one Department" };
            }
        }

        public BaseResponse AddOrUpdateServiceLine(ServiceLineVM serviceLine)
        {
            BaseResponse response = null;
            if (serviceLine.ServiceId > 0)
            {
                var service = _serviceRepo.Table.Where(x => x.IsDeleted != true && x.ServiceId == serviceLine.ServiceId).FirstOrDefault();
                if (service != null)
                {
                    service.ServiceName = serviceLine.ServiceName;
                    service.ModifiedBy = serviceLine.ModifiedBy;
                    service.ModifiedDate = DateTime.UtcNow;
                    _serviceRepo.Update(service);
                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = serviceLine };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
                }
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

        public BaseResponse DeleteServiceLine(int Id, int userId)
        {
            var service = _serviceRepo.Table.Where(x => x.ServiceId == Id).FirstOrDefault();
            if (service != null)
            {
                service.IsDeleted = true;
                service.ModifiedBy = userId;
                service.ModifiedDate = DateTime.UtcNow;
                _serviceRepo.Update(service);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        #endregion

        #region Department 

        public BaseResponse GetAllDepartments()
        {
            var departments = _departmentRepo.Table.Where(x => x.IsDeleted != true).ToList();
            var dpts = AutoMapperHelper.MapList<Department, DepartmentVM>(departments);
            var dptServices = (from ds in _departmentServiceRepo.Table
                               join s in _serviceRepo.Table on ds.ServiceIdFk equals s.ServiceId
                               where dpts.Select(x => x.DepartmentId).Contains(ds.DepartmentIdFk) && s.IsDeleted != true
                               select new ServiceLineVM()
                               {
                                   ServiceId = s.ServiceId,
                                   ServiceName = s.ServiceName,
                                   ServiceType = s.ServiceType,
                                   CreatedBy = s.CreatedBy,
                                   CreatedDate = s.CreatedDate,
                                   ModifiedBy = s.ModifiedBy,
                                   ModifiedDate = s.ModifiedDate,
                                   DepartmentIdFk = ds.DepartmentIdFk
                               }).Distinct().ToList();

            foreach (var item in dpts)
            {
                item.ServiceLines = dptServices.Where(x => x.DepartmentIdFk == item.DepartmentId).ToList();
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = dpts
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

        public BaseResponse GetDepartmentsByIds(string Ids)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                var idsList = Ids.ToIntList();
                var department = (from od in _organizationDepartmentRepo.Table
                                  join d in _departmentRepo.Table on od.DepartmentIdFk equals d.DepartmentId
                                  where d.IsDeleted != true && idsList.Contains(od.OrganizationIdFk)
                                  select new DepartmentVM()
                                  {
                                      DepartmentId = d.DepartmentId,
                                      DepartmentName = d.DepartmentName,
                                      OrganizationIdFk = od.OrganizationIdFk
                                  }).DistinctBy(x => x.DepartmentName).ToList();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = department };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Select at least one Organization" };
            }
        }

        public BaseResponse AddOrUpdateDepartment(DepartmentVM department)
        {
            BaseResponse response = null;

            var serviceIds = department.ServicesIdFks.ToIntList();
            List<DepartmentService> dptServices = new List<DepartmentService>();
            if (department.DepartmentId > 0)
            {
                var dpt = _departmentRepo.Table.Where(x => (x.IsDeleted == false || x.IsDeleted == null) && x.DepartmentId == department.DepartmentId).FirstOrDefault();
                if (dpt != null)
                {
                    dpt.DepartmentName = department.DepartmentName;
                    dpt.ModifiedBy = department.ModifiedBy;
                    dpt.ModifiedDate = DateTime.UtcNow;
                    _departmentRepo.Update(dpt);
                    var alreadyExistServices = _departmentServiceRepo.Table.Where(x => x.DepartmentIdFk == department.DepartmentId).ToList();
                    _departmentServiceRepo.DeleteRange(alreadyExistServices);
                    foreach (var item in serviceIds)
                    {
                        dptServices.Add(new DepartmentService() { ServiceIdFk = item, DepartmentIdFk = dpt.DepartmentId });
                    }
                    _departmentServiceRepo.Insert(dptServices);
                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated" };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
                }
            }
            else
            {
                var dpt = AutoMapperHelper.MapSingleRow<DepartmentVM, Department>(department);
                dpt.CreatedDate = DateTime.UtcNow;
                _departmentRepo.Insert(dpt);

                foreach (var item in serviceIds)
                {
                    dptServices.Add(new DepartmentService() { ServiceIdFk = item, DepartmentIdFk = dpt.DepartmentId });
                }
                _departmentServiceRepo.Insert(dptServices);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created" };
            }
            return response;
        }

        public BaseResponse DeleteDepartment(int Id, int userId)
        {
            var dpt = _departmentRepo.Table.Where(x => x.DepartmentId == Id).FirstOrDefault();
            if (dpt != null)
            {
                dpt.IsDeleted = true;
                dpt.ModifiedBy = userId;
                dpt.ModifiedDate = DateTime.UtcNow;
                _departmentRepo.Update(dpt);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }
        #endregion

        #region Organization 

        public BaseResponse GetAllOrganizations()
        {
            var organizations = _organizationRepo.Table.Where(x => x.IsDeleted == false).ToList();
            var orgs = AutoMapperHelper.MapList<Organization, OrganizationVM>(organizations);
            var dpts = (from od in _organizationDepartmentRepo.Table
                        join d in _departmentRepo.Table on od.DepartmentIdFk equals d.DepartmentId
                        where d.IsDeleted != true && orgs.Select(x => x.OrganizationId).Contains(od.OrganizationIdFk)
                        select new DepartmentVM()
                        {
                            DepartmentId = d.DepartmentId,
                            DepartmentName = d.DepartmentName,
                            CreatedBy = d.CreatedBy,
                            CreatedDate = d.CreatedDate,
                            ModifiedBy = d.ModifiedBy,
                            ModifiedDate = d.ModifiedDate,
                            IsDeleted = d.IsDeleted,
                            OrganizationIdFk = od.OrganizationIdFk
                        });
            var types = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var states = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.States.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in orgs)
            {
                item.State = states.Where(x => x.ControlListDetailId == item.StateIdFk).Select(x => x.Title).FirstOrDefault();
                item.OrgType = types.Where(x => x.ControlListDetailId == item.OrganizationType).Select(x => x.Title).FirstOrDefault();
                item.Departments = dpts.Where(x => x.OrganizationIdFk == item.OrganizationId).ToList();
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = orgs
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

            var dptIds = organization.DepartmentIdsFk.ToIntList();
            List<OrganizationDepartment> orgDpt = new List<OrganizationDepartment>();
            if (organization.OrganizationId > 0)
            {
                var org = _organizationRepo.Table.Where(x => x.IsDeleted != true && x.OrganizationId == organization.OrganizationId).FirstOrDefault();
                if (org != null)
                {
                    org.City = organization.City;
                    org.PhoneNo = organization.PhoneNo;
                    org.FaxNo = organization.FaxNo;
                    org.Zip = organization.Zip;
                    org.OrganizationType = organization.OrganizationType;
                    org.PrimaryAddress = organization.PrimaryAddress;
                    org.PrimaryAddress2 = organization.PrimaryAddress2;
                    org.StateIdFk = organization.StateIdFk;
                    org.OrganizationName = organization.OrganizationName;
                    org.ModifiedBy = organization.ModifiedBy;
                    org.ModifiedDate = DateTime.UtcNow;
                    _organizationRepo.Update(org);
                    var alreadyExistDpts = _organizationDepartmentRepo.Table.Where(x => x.OrganizationIdFk == organization.OrganizationId).ToList();
                    _organizationDepartmentRepo.DeleteRange(alreadyExistDpts);
                    foreach (var item in dptIds)
                    {
                        orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                    }
                    _organizationDepartmentRepo.Insert(orgDpt);

                    if (organization.ClinicalHours != null && organization.ClinicalHours.Count() > 0)
                    {
                        List<ClinicalHour> ClinicalHour = new List<ClinicalHour>();
                        foreach (var item in organization.ClinicalHours.Where(x => x.ClinicalHourId != 0))
                        {
                            var cHours = org.ClinicalHours.Where(x => x.OrganizationIdFk == organization.OrganizationId && x.IsDeleted != true).FirstOrDefault();
                            if (cHours != null) 
                            {
                                cHours.WeekDayIdFk = item.WeekDayIdFk;
                                cHours.StartDate = item.StartDate;
                                cHours.EndDate = item.EndDate;
                                cHours.StartTime = item.StartTime;
                                cHours.EndTime = item.EndTime;
                                cHours.ModifiedDate = DateTime.UtcNow;
                                cHours.ModifiedBy = item.ModifiedBy;
                                ClinicalHour.Add(cHours);
                            }
                        }
                        _clinicalHour.Update(ClinicalHour);
                        if (organization.ClinicalHours.Where(x => x.ClinicalHourId == 0).Count() > 0) 
                        {
                            ClinicalHour = new List<ClinicalHour>();
                            foreach (var item in organization.ClinicalHours.Where(x => x.ClinicalHourId == 0))
                            {
                                var clinicalHours = AutoMapperHelper.MapSingleRow<ClinicalHoursVM, ClinicalHour>(item);
                                clinicalHours.OrganizationIdFk = org.OrganizationId;
                                clinicalHours.CreatedDate = DateTime.UtcNow;
                                ClinicalHour.Add(clinicalHours);
                            }
                            _clinicalHour.Insert(ClinicalHour);
                        }
                    }

                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = organization };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
                }

            }
            else
            {
                var org = AutoMapperHelper.MapSingleRow<OrganizationVM, Organization>(organization);
                org.CreatedDate = DateTime.UtcNow;
                _organizationRepo.Insert(org);
                foreach (var item in dptIds)
                {
                    orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                }
                _organizationDepartmentRepo.Insert(orgDpt);

                if (organization.ClinicalHours.Count() > 0)
                {
                    List<ClinicalHour> ClinicalHour = new List<ClinicalHour>();
                    foreach (var item in organization.ClinicalHours)
                    {
                        var clinicalHours = AutoMapperHelper.MapSingleRow<ClinicalHoursVM, ClinicalHour>(item);
                        clinicalHours.OrganizationIdFk = org.OrganizationId;
                        clinicalHours.CreatedDate = DateTime.Now;
                        ClinicalHour.Add(clinicalHours);
                    }
                    _clinicalHour.Insert(ClinicalHour);
                }

                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = organization };
            }
            return response;
        }

        public BaseResponse DeleteOrganization(int Id, int userId)
        {
            var org = _organizationRepo.Table.Where(x => x.OrganizationId == Id).FirstOrDefault();
            if (org != null)
            {
                org.IsDeleted = true;
                org.ModifiedBy = userId;
                org.ModifiedDate = DateTime.UtcNow;
                _organizationRepo.Update(org);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }
        #endregion

    }
}

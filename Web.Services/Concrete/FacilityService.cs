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
        //private IRepository<DepartmentService> _departmentServiceRepo;
        //private IRepository<OrganizationDepartment> _organizationDepartmentRepo;
        private IRepository<ControlListDetail> _controlListDetails;

        public FacilityService(RAQ_DbContext dbContext,
            IRepository<ServiceLine> serviceRepo,
            IRepository<Department> departmentRepo,
            IRepository<Organization> organizationRepo,
            IRepository<ClinicalHour> clinicalHour,
            //IRepository<DepartmentService> departmentServiceRepo,
            //IRepository<OrganizationDepartment> organizationDepartmentRepo,
            IRepository<ControlListDetail> controlListDetails
            )
        {
            this._dbContext = dbContext;
            this._serviceRepo = serviceRepo;
            this._departmentRepo = departmentRepo;
            this._organizationRepo = organizationRepo;
            this._clinicalHour = clinicalHour;
            //this._departmentServiceRepo = departmentServiceRepo;
            //this._organizationDepartmentRepo = organizationDepartmentRepo;
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
            var serviceLine = _serviceRepo.Table.Where(x => x.ServiceLineId == Id && x.IsDeleted == false).FirstOrDefault();
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
                var services = this._serviceRepo.Table.Where(s => idsList.Contains(s.DepartmentIdFk) && s.IsDeleted != true).ToList();

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = services };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Select at least one Department" };
            }
        }
       
        public BaseResponse GetServicesByOrganizationId(int OrganizationId)
        {
            var services = _dbContext.LoadStoredProc("raq_getAllServicesByOrganizationId")
                .WithSqlParam("@pOrganizationId", OrganizationId)
            .ExecuteStoredProc<ServiceLineVM>().Result.ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = services };

        }

        public BaseResponse AddOrUpdateServiceLine(ServiceLineVM serviceLine)
        {
            BaseResponse response = null;
            if (serviceLine.ServiceLineId > 0)
            {
                var service = _serviceRepo.Table.Where(x => x.IsDeleted != true && x.ServiceLineId == serviceLine.ServiceLineId).FirstOrDefault();
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
            var service = _serviceRepo.Table.Where(x => x.ServiceLineId == Id).FirstOrDefault();
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
            var dptServices = (from s in this._serviceRepo.Table                              
                               //where dpts.Select(x => x.DepartmentId).Contains(ds.DepartmentIdFk) && s.IsDeleted != true
                               select new ServiceLineVM()
                               {
                                   ServiceLineId = s.ServiceLineId,
                                   ServiceName = s.ServiceName,
                                   ServiceType = s.ServiceType,
                                   CreatedBy = s.CreatedBy,
                                   CreatedDate = s.CreatedDate,
                                   ModifiedBy = s.ModifiedBy,
                                   ModifiedDate = s.ModifiedDate,
                                   DepartmentIdFk = s.DepartmentIdFk
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
        
        public BaseResponse GetAllDepartmentsByOrganizationId(int OrganizationId)
        {
            var departments = this._departmentRepo.Table.Where(od => od.OrganizationIdFk == OrganizationId && od.IsDeleted != true).ToList();
            var dpts = AutoMapperHelper.MapList<Department, DepartmentVM>(departments);
            var dptServices = (from s in this._serviceRepo.Table
                               where dpts.Select(x => x.DepartmentId).Contains(s.DepartmentIdFk) && s.IsDeleted != true 
                               select new ServiceLineVM()
                               {
                                   ServiceLineId = s.ServiceLineId,
                                   ServiceName = s.ServiceName,
                                   ServiceType = s.ServiceType,
                                   CreatedBy = s.CreatedBy,
                                   CreatedDate = s.CreatedDate,
                                   ModifiedBy = s.ModifiedBy,
                                   ModifiedDate = s.ModifiedDate,
                                   DepartmentIdFk = s.DepartmentIdFk
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
                var department = new DepartmentVM(); //this._departmentRepo.Table.Where(d=>d.IsDeleted!=true && idsList.Contains(d.OrganizationIdFk)).ToList()
                //    (from od in _organizationDepartmentRepo.Table
                //                  join d in _departmentRepo.Table on od.DepartmentIdFk equals d.DepartmentId
                //                  where d.IsDeleted != true && idsList.Contains(od.OrganizationIdFk)
                //                  select new DepartmentVM()
                //                  {
                //                      DepartmentId = d.DepartmentId,
                //                      DepartmentName = d.DepartmentName,
                //                      OrganizationIdFk = od.OrganizationIdFk
                //                  }).DistinctBy(x => x.DepartmentName).ToList();
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

            if (department.DepartmentId > 0)
            {
                var dpt = _departmentRepo.Table.Where(x => (x.IsDeleted != true) && x.DepartmentId == department.DepartmentId).FirstOrDefault();
                if (dpt != null)
                {
                    dpt.DepartmentName = department.DepartmentName;
                    dpt.ModifiedBy = department.ModifiedBy;
                    dpt.ModifiedDate = DateTime.UtcNow;
                    this._departmentRepo.Update(dpt);
                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated" };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
                }
            }
            else
            {
                var dpt =  AutoMapperHelper.MapSingleRow<DepartmentVM, Department>(department);
                    dpt.CreatedDate = DateTime.UtcNow;
                    this._departmentRepo.Insert(dpt);
               
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created" };
            }
            return response;
        }

        public BaseResponse DeleteDepartment(int departmentId, int userId, int organizationId)
        {
            var dpt = _departmentRepo.Table.Where(x => x.DepartmentId == departmentId && x.IsDeleted != true).FirstOrDefault();
            //var deptOrgRelation = this._organizationDepartmentRepo.Table.FirstOrDefault(r => r.DepartmentIdFk == departmentId && r.OrganizationIdFk == organizationId);
            if (dpt != null)
            {
                dpt.IsDeleted = true;
                dpt.ModifiedBy = userId;
                dpt.ModifiedDate = DateTime.UtcNow;
                _departmentRepo.Update(dpt);
                //this._organizationDepartmentRepo.Delete(deptOrgRelation);
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
            var departments = this._departmentRepo.Table.Where(d => d.IsDeleted != true && orgs.Select(x => x.OrganizationId).Contains(d.OrganizationIdFk)).ToList();
            var dpts = AutoMapperHelper.MapList<Department, DepartmentVM>(departments);

            var types = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var states = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.States.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in orgs)
            {
                item.State = states.Where(x => x.ControlListDetailId == item.StateIdFk).Select(x => x.Title).FirstOrDefault();
                item.OrgType = types.Where(x => x.ControlListDetailId == item.OrganizationType).Select(x => x.Title).FirstOrDefault();
                //item.Departments = dpts.Where(x => x.OrganizationIdFk == item.OrganizationId).ToList();
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

            //var dptIds = organization.DepartmentIdsFk.ToIntList();
            //List<OrganizationDepartment> orgDpt = new List<OrganizationDepartment>();
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

                    //var alreadyExistDpts = _organizationDepartmentRepo.Table.Where(x => x.OrganizationIdFk == organization.OrganizationId).ToList();
                    //_organizationDepartmentRepo.DeleteRange(alreadyExistDpts);
                    //foreach (var item in dptIds)
                    //{
                    //    orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                    //}
                    //_organizationDepartmentRepo.Insert(orgDpt);

                    //if (organization.ClinicalHours != null && organization.ClinicalHours.Count() > 0)
                    //{
                    //    List<ClinicalHour> ClinicalHour = new List<ClinicalHour>();
                    //    foreach (var item in organization.ClinicalHours.Where(x => x.ClinicalHourId != 0))
                    //    {
                    //        var cHours = org.ClinicalHours.Where(x => x.OrganizationIdFk == organization.OrganizationId && x.IsDeleted != true).FirstOrDefault();
                    //        if (cHours != null)
                    //        {
                    //            cHours.WeekDayIdFk = item.WeekDayIdFk;
                    //            cHours.StartDate = item.StartDate;
                    //            cHours.EndDate = item.EndDate;
                    //            cHours.StartTime = item.StartTime;
                    //            cHours.EndTime = item.EndTime;
                    //            cHours.ModifiedDate = DateTime.UtcNow;
                    //            cHours.ModifiedBy = item.ModifiedBy;
                    //            ClinicalHour.Add(cHours);
                    //        }
                    //    }
                    //    _clinicalHour.Update(ClinicalHour);
                    //    if (organization.ClinicalHours.Where(x => x.ClinicalHourId == 0).Count() > 0)
                    //    {
                    //        ClinicalHour = new List<ClinicalHour>();
                    //        foreach (var item in organization.ClinicalHours.Where(x => x.ClinicalHourId == 0))
                    //        {
                    //            var clinicalHours = AutoMapperHelper.MapSingleRow<ClinicalHoursVM, ClinicalHour>(item);
                    //            clinicalHours.OrganizationIdFk = org.OrganizationId;
                    //            clinicalHours.CreatedDate = DateTime.UtcNow;
                    //            ClinicalHour.Add(clinicalHours);
                    //        }
                    //        _clinicalHour.Insert(ClinicalHour);
                    //    }
                    //}

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
                //foreach (var item in dptIds)
                //{
                //    orgDpt.Add(new OrganizationDepartment() { OrganizationIdFk = org.OrganizationId, DepartmentIdFk = item });
                //}
                //_organizationDepartmentRepo.Insert(orgDpt);

                //if (organization.ClinicalHours != null && organization.ClinicalHours.Count() > 0)
                //{
                //    List<ClinicalHour> ClinicalHour = new List<ClinicalHour>();
                //    foreach (var item in organization.ClinicalHours)
                //    {
                //        var clinicalHours = AutoMapperHelper.MapSingleRow<ClinicalHoursVM, ClinicalHour>(item);
                //        clinicalHours.OrganizationIdFk = org.OrganizationId;
                //        clinicalHours.CreatedDate = DateTime.Now;
                //        ClinicalHour.Add(clinicalHours);
                //    }
                //    _clinicalHour.Insert(ClinicalHour);
                //}

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


        #region Clinical Hours 

        public BaseResponse GetAllClinicalHours()
        {
            var cHour = this._clinicalHour.Table.Where(x => x.IsDeleted != true).ToList();

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = cHour
            };
        }

        public BaseResponse GetClinicalHourById(int Id)
        {
            var cHour = this._clinicalHour.Table.Where(x => x.ClinicalHourId == Id && x.IsDeleted == false).FirstOrDefault();
            if (cHour != null)
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = cHour
                };
            }
            else
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Data Not Found"
                };
            }

        }

        public BaseResponse GetClinicalHourByServiceLineId(int orgId, int serviceLineId)
        {
            var cHours = (from ch in this._clinicalHour.Table
                          join sl in this._serviceRepo.Table on ch.ServicelineIdFk equals sl.ServiceLineId
                          join d in this._departmentRepo.Table on sl.DepartmentIdFk equals d.DepartmentId
                          join org in this._organizationRepo.Table on d.OrganizationIdFk equals org.OrganizationId
                          where
                          org.OrganizationId == orgId
                          && sl.ServiceLineId == serviceLineId
                          && org.IsDeleted != true
                          && d.IsDeleted != true
                          && sl.IsDeleted != true
                          && ch.IsDeleted != true
                          select ch).ToList();
                
                //this._clinicalHour.Table.Where(x => x.ServicelineIdFk == serviceLineId && x.IsDeleted == false).ToList();
            if (cHours != null && cHours.Count() > 0)
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = cHours
                };
            }
            else
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Data Not Found"
                };
            }

        }


        public BaseResponse AddOrUpdateClinicalHour(ClinicalHoursVM clinicalHours)
        {
            BaseResponse response = null;

            if (clinicalHours.ClinicalHourId > 0)
            {
                var cHour = this._clinicalHour.Table.Where(x => x.IsDeleted != true && x.ClinicalHourId == clinicalHours.ClinicalHourId).FirstOrDefault();
                if (cHour != null)
                {
                    cHour.WeekDayIdFk = clinicalHours.WeekDayIdFk;
                    cHour.ServicelineIdFk = clinicalHours.ServicelineIdFk;
                    cHour.StartDate = clinicalHours.StartDate;
                    cHour.StartTime = clinicalHours.StartTime;
                    cHour.StartBreak = clinicalHours.StartBreak;
                    cHour.EndDate = clinicalHours.EndDate;
                    cHour.EndTime = clinicalHours.EndTime;
                    cHour.EndBreak = clinicalHours.EndBreak;
                    cHour.ModifiedBy = clinicalHours.ModifiedBy;
                    cHour.ModifiedDate = DateTime.UtcNow;
                    cHour.IsDeleted = false;

                    this._clinicalHour.Update(cHour);

                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = cHour };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
                }

            }
            else
            {
                var chour = AutoMapperHelper.MapSingleRow<ClinicalHoursVM, ClinicalHour>(clinicalHours);
                chour.CreatedDate = DateTime.UtcNow;
                this._clinicalHour.Insert(chour);

                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = clinicalHours };
            }
            return response;
        }

        public BaseResponse DeleteClinicalHour(int Id, int userId)
        {
            var cHour = this._clinicalHour.Table.Where(x => x.ClinicalHourId == Id).FirstOrDefault();
            if (cHour != null)
            {
                cHour.IsDeleted = true;
                cHour.ModifiedBy = userId;
                cHour.ModifiedDate = DateTime.UtcNow;
                this._clinicalHour.Update(cHour);

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

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

        private IRepository<Role> _roleRepo;
        private IRepository<UsersRelation> _userRelationRepo;
        private IRepository<ServiceLine> _serviceRepo;
        private IRepository<Department> _departmentRepo;
        private IRepository<Organization> _organizationRepo;
        private IRepository<ClinicalHour> _clinicalHourRepo;
        private IRepository<ClinicalHoliday> _clinicalHolidayRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<UserRole> _userRoleRepo;

        public FacilityService(RAQ_DbContext dbContext,
            IRepository<Role> roleRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceRepo,
            IRepository<Department> departmentRepo,
            IRepository<Organization> organizationRepo,
            IRepository<ClinicalHour> clinicalHourRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<ClinicalHoliday> clinicalHolidayRepo,
            IRepository<UserRole> userRoleRepo
            )
        {
            this._dbContext = dbContext;
            this._roleRepo = roleRepo;
            this._userRelationRepo = userRelationRepo;
            this._serviceRepo = serviceRepo;
            this._departmentRepo = departmentRepo;
            this._organizationRepo = organizationRepo;
            this._clinicalHourRepo = clinicalHourRepo;
            this._controlListDetailsRepo = controlListDetailsRepo;
            this._clinicalHolidayRepo = clinicalHolidayRepo;
            this._userRoleRepo = userRoleRepo;
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
        public BaseResponse GetAllServiceLinesByDepartmentId(int DepartmentId, bool status)
        {
            var services = _serviceRepo.Table.Where(x => x.IsActive == status && x.DepartmentIdFk == DepartmentId && !x.IsDeleted).ToList();
            if (services.Count() > 0)
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = services
                };
            }
            else
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Services Not Found",
                    Body = services
                };
            }
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

        public BaseResponse getServicesByDepartmentIds(string departmentIds)
        {
            if (!string.IsNullOrEmpty(departmentIds))
            {
                var idsList = departmentIds.ToIntList();
                var services = _serviceRepo.Table.Where(x => idsList.Contains(x.DepartmentIdFk) && x.IsDeleted != true).Select(x => new ServiceLineVM()
                {
                    ServiceLineId = x.ServiceLineId,
                    ServiceName = x.ServiceName,
                    DepartmentIdFk = x.DepartmentIdFk
                }).ToList();
                //(from sl in _serviceRepo.Table
                //                join d in _departmentRepo.Table on sl.DepartmentIdFk equals d.DepartmentId
                //                join org in _organizationRepo.Table on d.OrganizationIdFk equals org.OrganizationId
                //                where idsList.Contains(sl.DepartmentIdFk) && sl.IsDeleted != true && d.IsDeleted != true && org.IsDeleted != true
                //                select new ServiceLineVM()
                //                {
                //                    ServiceLineId = sl.ServiceLineId,
                //                    ServiceName = sl.ServiceName,
                //                    DepartmentIdFk = d.DepartmentId
                //                }).ToList();

                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = services.Count() == 0 ? "Service Lines Not Found" : "Data Found",
                    Body = services
                };
            }
            else
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Select at least one Department"
                };
            }
        }

        public BaseResponse GetServicesByOrganizationId(int OrganizationId)
        {
            var services = _dbContext.LoadStoredProcedure("md_getAllServicesByOrganizationId")
                .WithSqlParam("@pOrganizationId", OrganizationId)
            .ExecuteStoredProc<ServiceLineVM>();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = services.Count() == 0 ? "Service Lines Not Found" : "Data Found",
                Body = services
            };

        }

        public BaseResponse AddOrUpdateServiceLine(List<ServiceLineVM> serviceLines)
        {
            BaseResponse response = null;
            foreach (var serviceLine in serviceLines)
            {
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
            }
            return response;
        }

        public BaseResponse ActiveOrInactiveServiceLine(int serviceLineId, int userId, bool status)
        {
            var service = _serviceRepo.Table.Where(x => x.ServiceLineId == serviceLineId).FirstOrDefault();
            if (service != null)
            {
                service.IsActive = status;
                service.ModifiedBy = userId;
                service.ModifiedDate = DateTime.UtcNow;
                _serviceRepo.Update(service);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = (status ? "Activate" : "InAvtivate") + " Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        public BaseResponse DeleteServiceLine(int serviceLineId, int userId, bool status)
        {
            var service = _serviceRepo.Table.Where(x => x.ServiceLineId == serviceLineId).FirstOrDefault();
            if (service != null)
            {
                service.IsDeleted = status;
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

        public BaseResponse GetAllDepartmentsByOrganizationId(int OrganizationId, bool status)
        {
            var departments = this._departmentRepo.Table.Where(od => od.OrganizationIdFk == OrganizationId && od.IsActive == status && od.IsDeleted == false).ToList();
            var dpts = AutoMapperHelper.MapList<Department, DepartmentVM>(departments);
            var dptServices = (from s in this._serviceRepo.Table
                               where dpts.Select(x => x.DepartmentId).Contains(s.DepartmentIdFk) && s.IsDeleted == false && s.IsActive == true
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
                var department = new List<DepartmentVM>();

                department = _departmentRepo.Table.Where(x => x.IsDeleted != true && idsList.Contains(x.OrganizationIdFk.Value)).Select(x => new DepartmentVM()
                {
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.DepartmentName,
                    OrganizationIdFk = x.OrganizationIdFk
                }).ToList();

                //this._departmentRepo.Table.Where(d=>d.IsDeleted!=true && idsList.Contains(d.OrganizationIdFk)).ToList()
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

        public BaseResponse AddOrUpdateDepartment(List<DepartmentVM> departments)
        {
            BaseResponse response = null;
            foreach (var department in departments)
            {

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
                    var dpt = AutoMapperHelper.MapSingleRow<DepartmentVM, Department>(department);
                    dpt.CreatedDate = DateTime.UtcNow;
                    dpt.CreatedBy = department.CreatedBy;
                    this._departmentRepo.Insert(dpt);

                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created" };
                }
            }
            return response;
        }

        public BaseResponse DeleteDepartment(int departmentId, int userId, bool status)
        {
            var dpt = _departmentRepo.Table.Where(x => x.DepartmentId == departmentId).FirstOrDefault();
            //var deptOrgRelation = this._organizationDepartmentRepo.Table.FirstOrDefault(r => r.DepartmentIdFk == departmentId && r.OrganizationIdFk == organizationId);
            if (dpt != null)
            {
                dpt.IsDeleted = status;
                dpt.ModifiedBy = userId;
                dpt.ModifiedDate = DateTime.UtcNow;
                _departmentRepo.Update(dpt);
                //this._organizationDepartmentRepo.Delete(deptOrgRelation);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = (status ? "Active" : "Inactive") + "Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }


        public BaseResponse ActiveOrInActiveDepartment(int departmentId, int userId, bool status)
        {
            var dpt = _departmentRepo.Table.Where(x => x.DepartmentId == departmentId).FirstOrDefault();
            //var deptOrgRelation = this._organizationDepartmentRepo.Table.FirstOrDefault(r => r.DepartmentIdFk == departmentId && r.OrganizationIdFk == organizationId);
            if (dpt != null)
            {
                dpt.IsActive = status;
                dpt.ModifiedBy = userId;
                dpt.ModifiedDate = DateTime.UtcNow;
                _departmentRepo.Update(dpt);
                //this._organizationDepartmentRepo.Delete(deptOrgRelation);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = (status ? "Active" : "Inactive") + "Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }
        #endregion

        #region Organization 

        public BaseResponse GetAllOrganizations(PaginationVM vM)
        {
            var orgs = this._dbContext.LoadStoredProcedure("md_getAllOrganizationsByRoleId_Dynamic")
                     .WithSqlParam("@UserId", ApplicationSettings.UserId)
                     .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                     .WithSqlParam("@page", vM.PageNumber)
                     .WithSqlParam("@size", vM.Rows)
                     .WithSqlParam("@sortOrder", vM.SortOrder)
                     .WithSqlParam("@sortCol", vM.SortCol)
                     .WithSqlParam("@filterVal", vM.FilterVal)
                     .ExecuteStoredProc<OrganizationVM>();

            var departments = this._departmentRepo.Table.Where(d => d.IsDeleted != true && orgs.Select(x => x.OrganizationId).Contains(d.OrganizationIdFk.Value)).ToList();
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
            orgs.ForEach(x => x.Departments = dpts.Where(d => d.OrganizationIdFk == x.OrganizationId).ToList());

            //var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            //var states = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.States.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            //foreach (var item in orgs)
            //{
            //    item.State = states.Where(x => x.ControlListDetailId == item.StateIdFk).Select(x => x.Title).FirstOrDefault();
            //    item.OrgType = types.Where(x => x.ControlListDetailId == item.OrganizationType).Select(x => x.Title).FirstOrDefault();
            //    item.Departments = dpts.Where(x => x.OrganizationIdFk == item.OrganizationId).ToList();
            //}


            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = orgs.Count() == 0 ? "Organization Not Found" : "Data Found",
                Body = orgs
            };
        }

        public BaseResponse GetOutpatientOrganizationsIvr()
        {
            var organization = new List<Organization>();
            if (ApplicationSettings.isSuperAdmin)
            {
                organization = _organizationRepo.Table.Where(x => x.IsDeleted == false).ToList();
            }
            else
            {
                organization = this._dbContext.LoadStoredProcedure("md_getAllOutpatientOrganizations")
                    .WithSqlParam("@UserId", ApplicationSettings.UserId)
                    .ExecuteStoredProc<Organization>().ToList();

            }

            var orgs = AutoMapperHelper.MapList<Organization, OrganizationVM>(organization);
            var departments = this._departmentRepo.Table.Where(d => d.IsDeleted != true && orgs.Select(x => x.OrganizationId).Contains(d.OrganizationIdFk.Value)).ToList();
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

            var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var states = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.States.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in orgs)
            {
                item.State = states.Where(x => x.ControlListDetailId == item.StateIdFk).Select(x => x.Title).FirstOrDefault();
                item.OrgType = types.Where(x => x.ControlListDetailId == item.OrganizationType).Select(x => x.Title).FirstOrDefault();
                item.Departments = dpts.Where(x => x.OrganizationIdFk == item.OrganizationId).ToList();
            }




            return new BaseResponse
            {
                Status = HttpStatusCode.OK,
                Message = orgs.Count() == 0 ? "Organization Not Found" : "Data Found",
                Body = orgs
            };
        }

        public BaseResponse GetAllOrganizations(bool status)
        {
            var organizations = new List<Organization>();
            if (ApplicationSettings.isSuperAdmin)
            {
                organizations = _organizationRepo.Table.Where(x => x.IsActive == status && !x.IsDeleted).ToList();
            }
            else
            {
                organizations = this._dbContext.LoadStoredProcedure("md_getAllOrganizationsByRoleId")
                    .WithSqlParam("@UserId", ApplicationSettings.UserId)
                    .ExecuteStoredProc<Organization>().ToList();
                /*(from ur in this._userRoleRepo.Table
                             join r in this._roleRepo.Table on ur.RoleIdFk equals r.RoleId
                             join o in this._organizationRepo.Table on r.OrganizationIdFk equals o.OrganizationId
                             where ur.UserIdFk == ApplicationSettings.UserId && !o.IsDeleted && !r.IsDeleted
                             select o).Distinct().ToList();*/
            }

            var orgs = AutoMapperHelper.MapList<Organization, OrganizationVM>(organizations);
            var departments = this._departmentRepo.Table.Where(d => d.IsDeleted != true && orgs.Select(x => x.OrganizationId).Contains(d.OrganizationIdFk.Value)).ToList();
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

            var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var states = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.States.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in orgs)
            {
                item.State = states.Where(x => x.ControlListDetailId == item.StateIdFk).Select(x => x.Title).FirstOrDefault();
                item.OrgType = types.Where(x => x.ControlListDetailId == item.OrganizationType).Select(x => x.Title).FirstOrDefault();
                item.Departments = dpts.Where(x => x.OrganizationIdFk == item.OrganizationId).ToList();
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = orgs.Count() == 0 ? "Organization Not Found" : "Data Found",
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

        public BaseResponse GetOrgAssociationTree(string Ids)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                var treeList = _dbContext.LoadStoredProcedure("md_getOrgAssociationList")
                .WithSqlParam("@pOrganizationId", Ids)
                .ExecuteStoredProc<TreeviewItemVM>();

                var orgTree = treeList.BuildTree();

                var roleTreeList = _dbContext.LoadStoredProcedure("md_getOrgsRole")
                .WithSqlParam("@pOrganizationId", Ids)
                .ExecuteStoredProc<TreeviewItemVM>();

                var roleTree = roleTreeList.BuildTree();

                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = new { orgTree, roleTree }
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
        public BaseResponse GetOrganizationTypeByOrgId(int orgId)
        {
            var organizationTypeId = this._organizationRepo.Table.Where(x => x.OrganizationId == orgId && x.IsDeleted == false).Select(x => x.OrganizationType).FirstOrDefault();
            IQueryable<OrganizationTypeVM> responseList = null;
            responseList = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new OrganizationTypeVM { OrganizationTypeId = x.ControlListDetailId, OrganizationTypeName = x.Title });
            if (organizationTypeId.Value == 83)
            {
                responseList = _controlListDetailsRepo.Table
                    .Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt() && x.IsDeleted != true && x.ControlListDetailId != 83)
                    .Select(x => new OrganizationTypeVM { OrganizationTypeId = x.ControlListDetailId, OrganizationTypeName = x.Title });
            }
            else
            {
                responseList = _controlListDetailsRepo.Table
                   .Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt() && x.IsDeleted != true && x.ControlListDetailId == organizationTypeId)
                   .Select(x => new OrganizationTypeVM { OrganizationTypeId = x.ControlListDetailId, OrganizationTypeName = x.Title });
            }

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = responseList
            };
        }

        public BaseResponse AddOrUpdateOrganization(OrganizationVM organization)
        {
            BaseResponse response = null;

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
                    org.OrganizationEmail = organization.OrganizationEmail;
                    org.PrimaryAddress = organization.PrimaryAddress;
                    org.PrimaryAddress2 = organization.PrimaryAddress2;
                    org.StateIdFk = organization.StateIdFk;
                    org.OrganizationName = organization.OrganizationName;
                    org.ActiveCodes = organization.ActiveCodes;
                    org.ModifiedBy = organization.ModifiedBy;
                    org.ModifiedDate = DateTime.UtcNow;
                    org.TimeZoneIdFk = organization.TimeZoneIdFk;
                    _organizationRepo.Update(org);

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

                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = organization };
            }
            return response;
        }

        public BaseResponse DeleteOrganization(int OrganizationId, bool status)
        {
            var org = _organizationRepo.Table.Where(x => x.OrganizationId == OrganizationId).FirstOrDefault();
            if (org != null)
            {
                //org.IsDeleted = true;
                org.IsActive = status;
                org.ModifiedBy = ApplicationSettings.UserId;
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
            var cHour = this._clinicalHourRepo.Table.Where(x => x.IsDeleted != true).ToList();

            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = cHour
            };
        }

        public BaseResponse GetClinicalHourById(int Id)
        {
            var cHour = this._clinicalHourRepo.Table.Where(x => x.ClinicalHourId == Id && x.IsDeleted == false).FirstOrDefault();
            if (cHour != null)
            {
                cHour.StartTime = cHour.StartTime.ToTimezoneFromUtc("Eastern Standard Time");
                cHour.EndTime = cHour.EndTime.ToTimezoneFromUtc("Eastern Standard Time");
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
            var cHours = (from ch in this._clinicalHourRepo.Table
                          join w in this._controlListDetailsRepo.Table on ch.WeekDayIdFk equals w.ControlListDetailId
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
                          select new clinicalHours()
                          {
                              id = ch.ClinicalHourId,
                              ServicelineIdFk = ch.ServicelineIdFk,
                              OrganizationId = org.OrganizationId,
                              day = ch.WeekDayIdFk,
                              startTime = ch.StartTime,
                              endTime = ch.EndTime,
                              startBreak = ch.StartBreak,
                              endBreak = ch.EndBreak,
                              WeekDay = w.Title,
                              CreatedBy = ch.CreatedBy,
                              CreatedDate = ch.CreatedDate

                          }).Distinct().ToList();

            //this._clinicalHour.Table.Where(x => x.ServicelineIdFk == serviceLineId && x.IsDeleted == false).ToList();
            if (cHours != null && cHours.Count() > 0)
            {
                foreach (var item in cHours)
                {
                    item.startTimeStr = item.startTime.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                    item.endTimeStr = item.endTime.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                }

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


        public BaseResponse AddOrUpdateClinicalHour(OrganizationSchedule clinicalHours)
        {
            BaseResponse response = null;
            ClinicalHour chour;
            clinicalHours.startTime = DateTime.Parse(clinicalHours.startTimeStr); //Convert.ToDateTime(schedule.StartTimeStr);
            clinicalHours.endTime = DateTime.Parse(clinicalHours.endTimeStr); //Convert.ToDateTime(schedule.EndTimeStr);
            if (clinicalHours.clinicalHourId > 0)
            {
                var cHour = this._clinicalHourRepo.Table.Where(x => x.IsDeleted != true && x.ClinicalHourId == clinicalHours.clinicalHourId).FirstOrDefault();
                if (cHour != null)
                {
                    chour = new ClinicalHour();

                    string startDateTimeStr = clinicalHours.startTime.ToString("MM-dd-yyyy") + " " + clinicalHours.startTime.ToString("hh:mm:ss tt");
                    string endDateTimeStr = clinicalHours.endTime.ToString("MM-dd-yyyy") + " " + clinicalHours.endTime.ToString("hh:mm:ss tt");

                    DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                    DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                    cHour.StartTime = StartDateTime.Value.ToUniversalTime();
                    cHour.EndTime = EndDateTime.Value.ToUniversalTime();
                    cHour.ModifiedBy = clinicalHours.modifiedBy;
                    cHour.ModifiedDate = DateTime.UtcNow;
                    cHour.IsDeleted = false;
                    this._clinicalHourRepo.Update(cHour);

                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = cHour };
                }

            }
            else
            {

                string[] serviceLineIds = clinicalHours.serviceLineIdFk.Split(",");
                foreach (var service in serviceLineIds)
                {
                    string[] weekDays = clinicalHours.weekDays.Split(",");
                    foreach (var item in weekDays)
                    {
                        var alreadExist = this._clinicalHourRepo.Table.Where(x => x.IsDeleted != true && x.ServicelineIdFk == service.ToInt() && x.WeekDayIdFk == item.ToInt()).FirstOrDefault();
                        if (alreadExist != null)
                        {
                            DeleteClinicalHour(alreadExist.ClinicalHourId, clinicalHours.createdBy);
                        }


                        string startDateTimeStr = clinicalHours.startTime.ToString("MM-dd-yyyy") + " " + clinicalHours.startTime.ToString("hh:mm:ss tt");
                        string endDateTimeStr = clinicalHours.endTime.ToString("MM-dd-yyyy") + " " + clinicalHours.endTime.ToString("hh:mm:ss tt");

                        DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                        DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);


                        chour = new ClinicalHour();
                        chour.ServicelineIdFk = service.ToInt();
                        chour.WeekDayIdFk = item.ToInt();
                        chour.StartTime = StartDateTime.Value.ToUniversalTime();
                        chour.EndTime = EndDateTime.Value.ToUniversalTime();
                        chour.CreatedBy = clinicalHours.createdBy;
                        chour.CreatedDate = DateTime.UtcNow;
                        chour.StartDate = StartDateTime.Value.ToUniversalTime();
                        chour.EndDate = EndDateTime.Value.ToUniversalTime();
                        chour.StartBreak = null;
                        chour.EndBreak = null;
                        chour.IsDeleted = false;
                        this._clinicalHourRepo.Insert(chour);
                    }
                }

                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = clinicalHours };

            }

            #region [commented]

            //if (clinicalHours != null && clinicalHours.organizationHours.Count > 0)
            //{
            //    for (int i = 0; i < clinicalHours.organizationHours.Count(); i++)
            //    {
            //        #region [Add or Update]

            //        var _clinicalHours = clinicalHours.organizationHours[i];
            //        if (_clinicalHours.id > 0)
            //        {

            //            var cHour = this._clinicalHourRepo.Table.Where(x => x.IsDeleted != true && x.ClinicalHourId == _clinicalHours.id).FirstOrDefault();
            //            if (cHour != null)
            //            {
            //                cHour.WeekDayIdFk = _clinicalHours.day;
            //                cHour.ServicelineIdFk = clinicalHours.serviceId;
            //                cHour.StartDate = _clinicalHours.startDate;
            //                cHour.StartTime = _clinicalHours.startTime;
            //                cHour.StartBreak = _clinicalHours.startBreak;
            //                cHour.EndDate = _clinicalHours.endDate;
            //                cHour.EndTime = _clinicalHours.endTime;
            //                cHour.EndBreak = _clinicalHours.endBreak;
            //                cHour.ModifiedBy = clinicalHours.LoggedInUserId;
            //                cHour.ModifiedDate = DateTime.UtcNow;
            //                cHour.IsDeleted = false;

            //                this._clinicalHourRepo.Update(cHour);

            //                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = cHour };
            //            }
            //            else
            //            {
            //                response = new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            //            }
            //        }
            //        else
            //        {
            //            chour = new ClinicalHour();
            //            chour.WeekDayIdFk = _clinicalHours.day;
            //            chour.ServicelineIdFk = clinicalHours.serviceId;
            //            chour.CreatedBy = clinicalHours.LoggedInUserId;
            //            chour.CreatedDate = DateTime.UtcNow;
            //            chour.IsDeleted = false;
            //            chour.StartDate = _clinicalHours.startDate.AddDays(1);
            //            chour.EndDate = _clinicalHours.endDate.AddDays(1);
            //            chour.StartTime = _clinicalHours.startTime;
            //            chour.EndTime = _clinicalHours.endTime;
            //            chour.StartBreak = _clinicalHours.startBreak;
            //            chour.EndBreak = _clinicalHours.endBreak;
            //            //var chour = AutoMapperHelper.MapSingleRow<clinicalHours, ClinicalHour>(_clinicalHours);
            //            this._clinicalHourRepo.Insert(chour);

            //            clinicalHours.organizationHours[i].id = chour.ClinicalHourId;

            //            response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Created", Body = clinicalHours };
            //        }

            //        #endregion

            //        #region [Delete]

            //        List<int> clinicalhoursIds = clinicalHours.organizationHours.Select(x => x.id).ToList();
            //        List<int> clinicalhoursDbIds = this._clinicalHourRepo.Table.Where(x => x.IsDeleted != true && x.ServicelineIdFk == clinicalHours.serviceId).Select(x => x.ClinicalHourId).ToList();
            //        List<int> deleteIds = clinicalhoursDbIds.Except(clinicalhoursIds).ToList();

            //        if (deleteIds.Count() > 0)
            //        {
            //            for (int ii = 0; ii < deleteIds.Count(); ii++)
            //            {
            //                var deleteId = deleteIds[ii];
            //                DeleteClinicalHour(deleteId, clinicalHours.LoggedInUserId);
            //            }

            //        }

            //        #endregion
            //    }
            //}
            //else
            //{
            //    var clinicHour = this._clinicalHourRepo.Table.Where(x => x.ServicelineIdFk == clinicalHours.serviceId).ToList();
            //    if (clinicHour != null && clinicHour.Count() > 0)
            //    {
            //        for (int ii = 0; ii < clinicHour.Count(); ii++)
            //        {
            //            var deleteId = clinicHour[ii];
            //            DeleteClinicalHour(deleteId.ClinicalHourId, clinicalHours.LoggedInUserId);
            //        }
            //    }

            //    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Deleted", Body = "" };
            //}

            #endregion

            return response;
        }

        public BaseResponse DeleteClinicalHour(int Id, int userId)
        {
            var cHour = this._clinicalHourRepo.Table.Where(x => x.ClinicalHourId == Id).FirstOrDefault();
            if (cHour != null)
            {
                cHour.IsDeleted = true;
                cHour.ModifiedBy = userId;
                cHour.ModifiedDate = DateTime.UtcNow;
                this._clinicalHourRepo.Update(cHour);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        #endregion

        #region Clinical Holidays

        public BaseResponse GetClinicalHolidayByServiceLineId(int serviceLineId)
        {
            var _List = this._clinicalHolidayRepo.Table.Where(ch => ch.IsDeleted != true && ch.ServicelineIdFk == serviceLineId);
            if (_List != null && _List.Count() > 0)
            {
                return new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = _List
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


        public BaseResponse SaveClinicalHoliday(ClinicalHolidayVM clinicalHoliday)
        {
            BaseResponse response = null;

            if (clinicalHoliday.ClinicalHolidayId > 0)
            {
                //update
                var holiday = this._clinicalHolidayRepo.Table.FirstOrDefault(h => h.IsDeleted != true && h.ClinicalHolidayId == clinicalHoliday.ClinicalHolidayId);
                if (holiday != null)
                {
                    var date = DateTime.Parse(clinicalHoliday.SelectedDateStr.ElementAt(0));

                    holiday.ServicelineIdFk = clinicalHoliday.ServicelineIdFk;
                    holiday.StartDate = date.ToUniversalTime().Date;
                    holiday.EndDate = date.ToUniversalTime().Date;
                    holiday.Description = clinicalHoliday.Description;
                    holiday.ModifiedBy = clinicalHoliday.ModifiedBy;
                    holiday.ModifiedDate = DateTime.UtcNow;
                    holiday.IsDeleted = false;

                    this._clinicalHolidayRepo.Update(holiday);
                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Updated", Body = "" };
                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Not Found", Body = "" };
                }
            }
            else
            {
                //insert
                List<ClinicalHoliday> clinicalHolidays = new();
                foreach (var item in clinicalHoliday.SelectedDateStr)
                {
                    ClinicalHoliday holiday = new ClinicalHoliday();

                    var date = DateTime.Parse(item);

                    holiday.ServicelineIdFk = clinicalHoliday.ServicelineIdFk;
                    holiday.StartDate = date.ToUniversalTime();
                    holiday.EndDate = date.ToUniversalTime();
                    holiday.Description = clinicalHoliday.Description;
                    holiday.CreatedBy = clinicalHoliday.CreatedBy;
                    holiday.CreatedDate = DateTime.UtcNow;
                    holiday.IsDeleted = false;
                    clinicalHolidays.Add(holiday);
                }

                if (clinicalHolidays.Count > 0)
                {
                    this._clinicalHolidayRepo.Insert(clinicalHolidays);
                }


                //var holiday = AutoMapperHelper.MapSingleRow<ClinicalHolidayVM, ClinicalHoliday>(clinicalHoliday);
                //holiday.CreatedDate = DateTime.UtcNow;
                //holiday.IsDeleted = false;
                //this._clinicalHolidayRepo.Insert(holiday);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully", Body = "" };
            }
            return response;
        }

        public BaseResponse DeleteClinicalHoliday(int clinicalHolidayId, int userId)
        {
            BaseResponse response = null;

            //update
            var holiday = this._clinicalHolidayRepo.Table.FirstOrDefault(h => h.IsDeleted != true && h.ClinicalHolidayId == clinicalHolidayId);
            if (holiday != null)
            {
                holiday.IsDeleted = true;
                holiday.ModifiedBy = userId;
                holiday.ModifiedDate = DateTime.UtcNow;

                this._clinicalHolidayRepo.Update(holiday);
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Successfully Deleted", Body = "" };
            }
            else
            {
                response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Not Found", Body = "" };
            }


            return response;
        }
        #endregion

    }
}

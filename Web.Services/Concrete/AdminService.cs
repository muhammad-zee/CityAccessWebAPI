﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
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
    public class AdminService : IAdminService
    {
        private RAQ_DbContext _dbContext;
        private IRepository<Component> _component;
        private IRepository<Role> _role;
        private IRepository<User> _user;
        private IRepository<ComponentAccess> _componentAccess;
        private IRepository<UserAccess> _userAccess;
        private IRepository<UserRole> _userRole;
        private IRepository<UsersSchedule> _userScheduleRepo;

        private IRepository<ServiceLine> _serviceRepo;
        private IRepository<Department> _departmentRepo;
        private IRepository<Organization> _organizationRepo;
        private IRepository<ControlList> _controlList;
        private IRepository<ControlListDetail> _controlListDetails;
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<FavouriteTeam> _favouriteTeam;

        IConfiguration _config;
        public AdminService(RAQ_DbContext dbContext,
            IConfiguration config,
            IRepository<Component> component,
            IRepository<ComponentAccess> componentAccess,
            IRepository<User> user,
            IRepository<Role> role,
            IRepository<UserRole> userRole,
            IRepository<UsersSchedule> userScheduleRepo,
            IRepository<ServiceLine> serviceRepo,
            IRepository<Department> departmentRepo,
            IRepository<Organization> organizationRepo,
            IRepository<UserAccess> userAccess,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ControlList> controlList,
            IRepository<FavouriteTeam> favouriteTeam,
            IRepository<ControlListDetail> controlListDetails)
        {
            this._dbContext = dbContext;
            this._componentAccess = componentAccess;
            this._component = component;
            this._config = config;
            this._component = component;
            this._user = user;
            this._role = role;
            this._userRole = userRole;
            this._userScheduleRepo = userScheduleRepo;
            this._serviceRepo = serviceRepo;
            this._departmentRepo = departmentRepo;
            this._organizationRepo = organizationRepo;
            this._userAccess = userAccess;
            this._userRelationRepo = userRelationRepo;
            this._favouriteTeam = favouriteTeam;
            this._controlList = controlList;
            this._controlListDetails = controlListDetails;
        }


        #region Dashboard Data

        public BaseResponse GetLabelCounts(int orgId)
        {
            var LabelCounts = this._dbContext.LoadStoredProcedure("md_getCountOfTablesForDashboard")
                    //.WithSqlParam("@is_SuperAdmin", ApplicationSettings.isSuperAdmin)
                    //.WithSqlParam("@userId", ApplicationSettings.UserId)
                    .WithSqlParam("@orgId", orgId)
                    .ExecuteStoredProc<TableCountsForDashBoardVM>().FirstOrDefault();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = LabelCounts };
        }

        public BaseResponse GetUsersForDashBoard(int orgId)
        {
            var users = this._dbContext.LoadStoredProcedure("md_getUsersForDashBoard")
                    .WithSqlParam("@is_SuperAdmin", ApplicationSettings.isSuperAdmin)
                    .WithSqlParam("@userId", ApplicationSettings.UserId)
                    .WithSqlParam("@orgId", orgId)
                    .ExecuteStoredProc<RegisterCredentialVM>();


            users.ForEach(x =>
            {
                x.UserRole = (from ur in _userRole.Table
                              join r in _role.Table on ur.RoleIdFk equals r.RoleId
                              //join o in _organizationRepo.Table on r.OrganizationIdFk equals o.OrganizationId
                              where ur.UserIdFk == x.UserId && !r.IsDeleted
                              //&& !o.IsDeleted
                              select new UserRoleVM()
                              {
                                  RoleId = r.RoleId,
                                  //RoleName = r.RoleName + " | " + o.OrganizationName
                                  RoleName = r.RoleName,
                              }).Distinct().ToList();
                x.UserServices = (from ur in _userRelationRepo.Table
                                  join s in _serviceRepo.Table on ur.ServiceLineIdFk equals s.ServiceLineId
                                  where ur.UserIdFk == x.UserId && !s.IsDeleted
                                  select new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).Distinct().ToList();

                x.Organizations = (from ur in _userRole.Table
                                   join r in _role.Table on ur.RoleIdFk equals r.RoleId
                                   join o in _organizationRepo.Table on r.OrganizationIdFk equals o.OrganizationId
                                   where ur.UserIdFk == x.UserId && !o.IsDeleted
                                   select new OrganizationVM() { OrganizationId = o.OrganizationId, OrganizationName = o.OrganizationName }).Distinct().ToList();
            });

            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }

        public BaseResponse GetSchedulesForCurrentDate(ScheduleVM schedule)
        {
            var scheduleList = this._dbContext.LoadStoredProcedure("md_getScheduleListForDashboard")
                            .WithSqlParam("@orgId", schedule.selectedOrganizationId)
                            .WithSqlParam("@serviceLineIds", schedule.selectedService)
                            .WithSqlParam("@roleIds", schedule.selectedRole)
                            .WithSqlParam("@userIds", schedule.selectedUser)
                            .ExecuteStoredProc<ScheduleListVM>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = scheduleList };
        }


        #endregion


        #region Users

        public BaseResponse GetAllUsers()
        {
            var result = _user.Table.Where(x => x.IsDeleted == false).ToList();
            var users = AutoMapperHelper.MapList<User, RegisterCredentialVM>(result);
            var userRelationIds = this._userRelationRepo.GetList();
            var genders = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.Genders.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in users)
            {

                item.UserRole = getRoleListByUserId(item.UserId).ToList();
                item.GenderId = Convert.ToInt32(item.Gender);
                item.Gender = genders.Where(x => x.ControlListDetailId == item.GenderId).Select(x => x.Title).FirstOrDefault();
                item.UserImage = String.IsNullOrEmpty(item.UserImage) ? "" : item.UserImage.Replace(Directory.GetCurrentDirectory() + "/", "");
                item.DobStr = item.Dob?.ToString("yyyy-MM-dd");
                var dptids = _serviceRepo.Table.Where(x => userRelationIds.Where(x => x.UserIdFk == item.UserId).Select(x => x.ServiceLineIdFk).Distinct().Contains(x.ServiceLineId) && x.IsDeleted != true).Select(x => x.DepartmentIdFk).Distinct().ToList();
                item.ServiceLineIdsList = userRelationIds.Where(x => x.UserIdFk == item.UserId).Select(x => x.ServiceLineIdFk).Distinct().ToList();
                item.DptIdsList = dptids;
                item.OrgIdsList = _departmentRepo.Table.Where(x => x.IsDeleted != true && dptids.Contains(x.DepartmentId)).Select(x => x.OrganizationIdFk.Value).Distinct().ToList();

            }
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }
        public BaseResponse GetAllUsersByOrganizationId(int OrganizationId, int UserRoleId)
        {
            //var result = _user.Table.Where(x => x.IsDeleted == false).ToList();
            var result = this._dbContext.LoadStoredProcedure("md_getAllUsersByOrganizationId")
                .WithSqlParam("@pOrganizationId", OrganizationId)
                .WithSqlParam("@pUserRoleId", UserRoleId)
                .ExecuteStoredProc<User>();

            var users = AutoMapperHelper.MapList<User, RegisterCredentialVM>(result);
            var userRelationIds = this._userRelationRepo.GetList();
            var genders = _controlListDetails.Table.Where(x => x.ControlListIdFk == UCLEnums.Genders.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            foreach (var item in users)
            {
                item.UserRole = getRoleListByUserId(item.UserId).ToList();
                item.GenderId = Convert.ToInt32(item.Gender);
                item.Gender = genders.Where(x => x.ControlListDetailId == item.GenderId).Select(x => x.Title).FirstOrDefault();
                item.UserImage = String.IsNullOrEmpty(item.UserImage) ? "" : item.UserImage.Replace(Directory.GetCurrentDirectory() + "/", "");
                item.DobStr = item.Dob?.ToString("yyyy-MM-dd");
                var dptids = _serviceRepo.Table.Where(x => userRelationIds.Where(x => x.UserIdFk == item.UserId).Select(x => x.ServiceLineIdFk).Distinct().ToList().Contains(x.ServiceLineId) && x.IsDeleted != true).Select(x => x.DepartmentIdFk).Distinct().ToList();
                item.ServiceLineIdsList = userRelationIds.Where(x => x.UserIdFk == item.UserId).Select(x => x.ServiceLineIdFk).Distinct().ToList();
                item.DptIdsList = dptids;
                item.OrgIdsList = item.ServiceLineIdsList.Count > 0 ? _departmentRepo.Table.Where(x => x.IsDeleted != true && dptids.Contains(x.DepartmentId)).Select(x => x.OrganizationIdFk.Value).Distinct().ToList() : this._role.Table.Where(x => item.UserRole.Select(r => r.RoleId).Contains(x.RoleId) && !x.IsDeleted && x.OrganizationIdFk.HasValue).Select(x => x.OrganizationIdFk.Value).Distinct().ToList();
                item.serviceIdsFT = _favouriteTeam.Table.Where(x => x.UserIdFk == item.UserId).Select(x => x.ServiceLineIdFk).ToList();
                item.dptIdsFT = _serviceRepo.Table.Where(x => item.serviceIdsFT.Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => x.DepartmentIdFk).Distinct().ToList();
                item.orgIdsFT = _departmentRepo.Table.Where(x => item.dptIdsFT.Contains(x.DepartmentId) && !x.IsDeleted).Select(x => x.OrganizationIdFk).Distinct().ToList();
            }
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }
        public BaseResponse GetAllUsersByServiceLineAndRoleId(string OrganizationId, string ServiceLineId, string RoleIds)
        {
            var users = _dbContext.LoadStoredProcedure("md_getAllUsersByServiceLineIdAndRoleId")
                        .WithSqlParam("@organizationId", OrganizationId)
                        .WithSqlParam("@serviceLineId", ServiceLineId)
                        .WithSqlParam("@roleId", RoleIds).ExecuteStoredProc<UserListVm>().ToList();

            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }
        public BaseResponse getAllScheduleUsersByServiceAndRoleId(string OrganizationId, string ServiceLineId, string RoleIds)
        {
            var users = _dbContext.LoadStoredProcedure("md_getAllScheduleUsersByServiceLineIdAndRoleId")
                        .WithSqlParam("@organizationId", OrganizationId)
                        .WithSqlParam("@serviceLineId", ServiceLineId)
                        .WithSqlParam("@roleId", RoleIds).ExecuteStoredProc<UserListVm>().ToList();

            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }

        public BaseResponse GetUserById(int Id)
        {
            var USER = _user.Table.Where(x => x.UserId == Id && x.IsDeleted == false).FirstOrDefault();
            var genders = _controlListDetails.Table.Select(x => new { x.ControlListDetailId, x.Title });
            if (USER != null)
            {
                var user = AutoMapperHelper.MapSingleRow<User, RegisterCredentialVM>(USER);
                user.UserRole = getRoleListByUserId(Id).ToList();
                user.GenderId = Convert.ToInt32(user.Gender);
                user.Gender = genders.Where(x => x.ControlListDetailId == user.GenderId).Select(x => x.Title).FirstOrDefault();
                user.UserImage = user.UserImage?.Replace(Directory.GetCurrentDirectory() + "/", "");
                user.DobStr = user.Dob?.ToString("yyyy-MM-dd");
                user.State = _controlListDetails.Table.Where(x => x.ControlListDetailId == user.StateKey).Select(x => x.Title).FirstOrDefault();
                user.UserServices = (from us in _userRelationRepo.Table
                                     join s in _serviceRepo.Table on us.ServiceLineIdFk equals s.ServiceLineId
                                     where us.UserIdFk == user.UserId && !s.IsDeleted
                                     select new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, DepartmentIdFk = s.DepartmentIdFk }).ToList();
                user.Departments = _departmentRepo.Table.Where(x => user.UserServices.Select(y => y.DepartmentIdFk).Contains(x.DepartmentId) && !x.IsDeleted).Select(x => new DepartmentVM() { DepartmentId = x.DepartmentId, DepartmentName = x.DepartmentName, OrganizationIdFk = x.OrganizationIdFk }).ToList();
                user.Organizations = user.Departments.Count > 0 ? _organizationRepo.Table.Where(x => user.Departments.Select(z => z.OrganizationIdFk).Contains(x.OrganizationId)).Select(x => new OrganizationVM() { OrganizationId = x.OrganizationId, OrganizationName = x.OrganizationName }).ToList() : this._organizationRepo.Table.Where(x => this._role.Table.Where(r => user.UserRole.Select(id => id.RoleId).Contains(r.RoleId) && !r.IsDeleted).Select(r => r.OrganizationIdFk).Contains(x.OrganizationId) && !x.IsDeleted).Select(x => new OrganizationVM() { OrganizationId = x.OrganizationId, OrganizationName = x.OrganizationName }).ToList();

                var schedule = this._userScheduleRepo.Table.Where(x => user.UserRole.Select(r => r.RoleId).Contains(x.RoleIdFk) && user.UserServices.Select(u => u.ServiceLineId).Contains(x.ServiceLineIdFk.Value) && x.UserIdFk == Id && x.ScheduleDate.Value.Date >= DateTime.UtcNow.Date && !x.IsDeleted).ToList();

                List<UserProfileSchedulesVM> UserProfileSchedules = new();
                foreach (var item in schedule)
                {
                    UserProfileSchedulesVM userProfile = new();
                    userProfile.RoleName = user.UserRole.Where(x => x.RoleId == item.RoleIdFk).Select(x => x.RoleName).FirstOrDefault();
                    userProfile.ServiceLine = user.UserServices.Where(x => x.ServiceLineId == item.ServiceLineIdFk).Select(x => x.ServiceName).FirstOrDefault();
                    userProfile.ScheduleDate = item.ScheduleDate;
                    userProfile.StartTime = item.ScheduleDateStart;
                    userProfile.EndTime = item.ScheduleDateEnd;
                    userProfile.DayOfWeek = item.ScheduleDate.Value.Date == DateTime.UtcNow.Date ? "Today" : (item.ScheduleDate.Value.Date == DateTime.UtcNow.AddDays(1).Date ? "Tomorrow" : item.ScheduleDate.Value.DayOfWeek.ToString());
                    UserProfileSchedules.Add(userProfile);
                }

                user.UserProfileSchedules = UserProfileSchedules.OrderBy(x=>x.ScheduleDate).ToList();

                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User Found", Body = user };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
            }

        }

        public BaseResponse DeleteUser(int Id)
        {
            var USER = _user.Table.Where(x => x.UserId == Id && x.IsDeleted == false).FirstOrDefault();
            if (USER != null)
            {
                USER.IsDeleted = true;
                USER.ModifiedDate = DateTime.UtcNow;
                _user.Update(USER);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User Deleted" };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
            }

        }

        #endregion

        #region UserRole

        public BaseResponse GetUsersByRoleId(int roleId)
        {
            var roleUsers = (from ur in _userRole.Table
                             join u in _user.Table on ur.UserIdFk equals u.UserId
                             where ur.RoleIdFk == roleId && u.IsDeleted == false
                             select new
                             {
                                 UserId = u.UserId,
                                 Name = u.FirstName + " " + u.LastName
                             }).OrderBy("Name").ToList();
            //var roleUsersVM = AutoMapperHelper.MapList<User, RegisterCredentialVM>(roleUsers);
            var response = new BaseResponse();

            if (roleUsers.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = roleUsers,
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Users not Found",
                    Body = new List<string>()
                };
            }

            return response;
        }

        #endregion

        #region Roles
        public IQueryable<Role> getRoleList()
        {
            return this._role.GetList().Where(item => !item.IsDeleted);
        }
        public IQueryable<Role> getRoleListByOrganizationId(int OrganizationId)
        {
            var rolesList = this._role.Table.Where(item => item.OrganizationIdFk == OrganizationId && !item.IsDeleted && !item.IsSuperAdmin);
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    var superAdminRole = this._role.GetList().Where(item => item.RoleId == userRoleId);
            //    rolesList = superAdminRole.Union(rolesList);
            //}
            return rolesList;
        }
        public IQueryable<Role> getScheduleRoleListByOrganizationId(int OrganizationId)
        {
            var rolesList = this._role.Table.Where(item => item.OrganizationIdFk == OrganizationId && !item.IsDeleted && !item.IsSuperAdmin && item.IsScheduleRequired);
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    var superAdminRole = this._role.GetList().Where(item => item.RoleId == userRoleId);
            //    rolesList = superAdminRole.Union(rolesList);
            //}
            return rolesList;
        }

        public IQueryable<Role> getRoleListByOrganizationIds(string OrganizationIds)
        {
            var ids = OrganizationIds.ToIntList();
            var roleList = (from r in this._role.Table
                            join org in this._organizationRepo.Table on r.OrganizationIdFk equals org.OrganizationId
                            where ids.Contains(org.OrganizationId) && r.IsDeleted != true && org.IsDeleted != true
                            select new Role()
                            {
                                RoleId = r.RoleId,
                                RoleName = ids.Count > 1 ? org.OrganizationName + " | " + r.RoleName : r.RoleName,
                                RoleDescription = r.RoleDescription,
                                IsDeleted = r.IsDeleted,
                                OrganizationIdFk = r.OrganizationIdFk
                            }).OrderBy(x => x.RoleName);
            //this._role.Table.Where(x => ids.Contains(x.OrganizationIdFk.Value) && x.IsDeleted != true);
            return roleList;
        }


        public List<UserRoleVM> getRoleListByUserId(int UserId)
        {
            var userRoles = (from ur in this._userRole.Table
                             join r in this._role.Table on ur.RoleIdFk equals r.RoleId
                             where ur.UserIdFk == UserId && !r.IsDeleted
                             select new UserRoleVM
                             {
                                 //UserRoleId = ur.UserRoleId,
                                 //UserId = ur.UserIdFK,
                                 RoleId = ur.RoleIdFk,
                                 RoleName = r.RoleName,
                                 OrganizationIdFk = r.OrganizationIdFk
                             }).ToList();
            foreach (var item in userRoles)
            {
                string orgName = this._organizationRepo.Table.Where(x => x.OrganizationId == item.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                if (!string.IsNullOrEmpty(orgName) && !string.IsNullOrWhiteSpace(orgName))
                    item.RoleName = item.RoleName + " | " + orgName;
            }
            return userRoles;
        }
        public string SaveRole(List<RoleVM> roles)
        {
            string response = string.Empty;

            foreach (var role in roles)
            {
                if (role.RoleId == 0)
                {
                    var newRole = AutoMapperHelper.MapSingleRow<RoleVM, Role>(role);
                    if (_role.Table.Count(r => r.RoleName == role.RoleName && r.OrganizationIdFk == role.OrganizationIdFk && !r.IsDeleted) == 0)
                    {
                        _role.Insert(newRole);
                        response = StatusEnums.Success.ToString();
                    }
                    else
                    {
                        response = StatusEnums.AlreadyExist.ToString();
                    }

                }
                else
                {
                    var newRole = _role.Table.Where(r => r.RoleId == role.RoleId && !r.IsDeleted).FirstOrDefault();
                    newRole.RoleName = role.RoleName;
                    newRole.RoleDescription = role.RoleDescription;
                    newRole.RoleDiscrimination = role.RoleDiscrimination;
                    newRole.IsScheduleRequired = role.IsScheduleRequired;
                    newRole.OrganizationIdFk = role.OrganizationIdFk;
                    newRole.ModifiedDate = DateTime.UtcNow;
                    newRole.ModifiedBy = role.ModifiedBy;

                    _role.Update(newRole);
                    response = StatusEnums.Success.ToString();
                }

            }

            return response;
        }

        public BaseResponse DeleteRole(int Id)
        {
            var Role = _role.Table.Where(x => x.RoleId == Id && x.IsDeleted == false).FirstOrDefault();
            if (Role != null)
            {
                Role.IsDeleted = true;
                _role.Update(Role);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Role Deleted" };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "Role Not Found" };
            }
        }

        #endregion

        #region Component

        public BaseResponse AddOrUpdateComponent(List<ComponentVM> components)
        {
            var comp_Controllers = components.Select(x => x.ComModuleName).Distinct().ToList();
            var comp_Action = components.Select(x => new { x.ComModuleName, x.PageName, x.PageDescription }).ToList();

            var comp_db_list = _component.GetList().ToList();
            List<Component> comps = new List<Component>();
            foreach (var item in comp_Controllers)
            {
                var cmp = comp_db_list.Where(x => x.ComModuleName == item).FirstOrDefault();
                if (cmp == null)
                {
                    comps.Add(new Component()
                    {
                        ComModuleName = item,
                        CreatedBy = components.Where(x => x.CreatedBy != 0).Select(x => x.CreatedBy).FirstOrDefault(),
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            _component.Insert(comps);
            comps = new List<Component>();
            comp_db_list = _component.GetList().ToList();
            foreach (var item in comp_Action)
            {
                var cmpId = comp_db_list.Where(x => x.ComModuleName == item.ComModuleName).Select(x => x.ComponentId).FirstOrDefault();
                if (cmpId > 0)
                {
                    var cmp = comp_db_list.Where(x => x.PageName == item.PageName).FirstOrDefault();
                    if (cmp == null)
                    {
                        comps.Add(new Component()
                        {
                            ParentComponentId = cmpId,
                            ComModuleName = item.PageName,
                            PageDescription = item.PageDescription,
                            CreatedBy = components.Where(x => x.CreatedBy != 0).Select(x => x.CreatedBy).FirstOrDefault(),
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }
            }
            _component.Insert(comps);

            var controllers = components.Select(x => x.ComModuleName).Distinct().ToList();
            var Action = components.Select(x => x.PageName).ToList();

            var extras = _component.Table.Where(x => !(controllers.Contains(x.ComModuleName) && x.ParentComponentId == null) && !(Action.Contains(x.ComModuleName) && x.ParentComponentId != null)).ToList();
            if (extras.Count() > 0)
            {
                _component.DeleteRange(extras);
            }

            BaseResponse response = new BaseResponse()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = StatusEnums.Created.ToString(),
                Body = _component.GetList().ToList(),
            };

            return response;
        }

        #endregion

        #region Component Access

        public BaseResponse GetAllComponents()
        {
            var response = new BaseResponse();
            var result = _component.Table.Where(x => !x.IsDeleted).ToList();
            if (result.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = result,
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.NotFound,
                    Message = "Data not Found",
                    Body = null,
                };
            }

            return response;
        }

        public BaseResponse GetComponentById(int Id)
        {
            var response = new BaseResponse();
            var result = _component.Table.Where(x => x.ComponentId == Id && x.IsDeleted == false).FirstOrDefault();
            if (result != null)
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = result,
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.NotFound,
                    Message = "Data not Found",
                    Body = null,
                };
            }
            return response;
        }

        public TreeviewItemVM[] GetAllModuleMenu(int Id)
        {
            var result = _component.Table.Where(x => x.Status == true).ToList();
            int parentComponentCount = result.Where(x => x.ParentComponentId == null).Count();
            var ParentComponents = result.Where(X => X.ParentComponentId == null);
            TreeviewItemVM[] TreeviewItemVM = new TreeviewItemVM[parentComponentCount];
            int iteration = 0;


            foreach (var pItem in result.Where(x => x.ParentComponentId == null).OrderBy(x => x.SortOrder))
            {
                var iCategory = new TreeviewItemVM();
                iCategory.label = pItem.ComModuleName;
                iCategory.key = pItem.ComponentId.ToString();
                iCategory.@checked = (_componentAccess.Table.Where(x => x.ComponentIdFk == pItem.ComponentId && x.RoleIdFk == Id).FirstOrDefault() != null) ? true : false;
                iCategory.expanded = false;
                iCategory.disabled = false;
                int ChildComponentsCount = result.Where(x => x.ParentComponentId == pItem.ComponentId).Count();
                if (ChildComponentsCount > 0)
                {
                    iCategory.children = new List<TreeviewItemVM>();

                }

                foreach (var cItem in result.Where(x => x.ParentComponentId == pItem.ComponentId).OrderBy(x => x.SortOrder))
                {
                    TreeviewItemVM cCategory = new TreeviewItemVM();

                    cCategory.label = cItem.ComModuleName;
                    cCategory.key = cItem.ComponentId.ToString();
                    cCategory.@checked = (_componentAccess.Table.Where(x => x.ComponentIdFk == cItem.ComponentId && x.RoleIdFk == Id).FirstOrDefault() != null) ? true : false; ;
                    cCategory.expanded = false;
                    cCategory.disabled = false;
                    int Child2ComponentsCount = result.Where(x => x.ParentComponentId == null).Count();
                    if (ChildComponentsCount > 0)
                    {
                        cCategory.children = new List<TreeviewItemVM>();
                    }
                    foreach (var c2Item in result.Where(X => X.ParentComponentId == cItem.ComponentId).OrderBy(x => x.SortOrder))
                    {
                        TreeviewItemVM c2Category = new TreeviewItemVM();
                        c2Category.label = c2Item.ComModuleName;
                        c2Category.key = c2Item.ComponentId.ToString();
                        c2Category.@checked = (_componentAccess.Table.Where(x => x.ComponentIdFk == c2Item.ComponentId && x.RoleIdFk == Id).FirstOrDefault() != null) ? true : false; ;
                        c2Category.expanded = false;
                        c2Category.disabled = false;

                        cCategory.children.Add(c2Category);
                    }

                    iCategory.children.Add(cCategory);
                }
                TreeviewItemVM[iteration] = iCategory;
                iteration++;
            }
            return TreeviewItemVM;
        }

        public BaseResponse GetComponentsTreeByRoleId(int Id)
        {

            var response = new BaseResponse();
            //<--------Make a join of ComponentAccess Table with Component to get List of Accessible Components By Role Id----------->
            var result = _component.Table.Where(x => x.Status == true).ToList();
            var treeItems = result.Select(x => new TreeviewItemVM()
            {
                key = x.ComponentId.ToString(),
                ParentKey = x.ParentComponentId,
                label = x.ComModuleName,
                sortOrder = x.SortOrder,
                icon = x.ModuleImage,
                expanded = true,
            }).OrderBy(x => x.sortOrder).ToList();
            var treeViewItems = treeItems.BuildTree();
            var selectedRoleAccessIds = _componentAccess.Table.Where(x => x.RoleIdFk == Id && x.IsActive == true).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList();

            if (treeViewItems.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = new { TreeViewItems = treeViewItems, SelectedIds = selectedRoleAccessIds },
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Data not Found",
                    Body = null,
                };
            }
            return response;
        }

        public BaseResponse GetComponentsTreeByUserRoleId(int roleId, int userId)
        {

            var response = new BaseResponse();
            var result = _component.Table.Where(x => x.Status == true).ToList();
            var treeItems = result.Select(x => new TreeviewItemVM()
            {
                key = x.ComponentId.ToString(),
                ParentKey = x.ParentComponentId,
                label = x.ComModuleName,
                sortOrder = x.SortOrder,
                icon = x.ModuleImage,
                expanded = true,
            }).OrderBy(x => x.sortOrder).ToList();
            var treeViewItems = treeItems.BuildTree();
            //<--------Make a join of ComponentAccess Table with Component to get List of Accessible Components By Role and user Id----------->
            var selectedUserRoleAccessIds = _componentAccess.Table.Where(x => x.RoleIdFk == roleId && x.IsActive == true).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList();
            selectedUserRoleAccessIds.AddRange(_userAccess.Table.Where(x => x.RoleIdFk == roleId && x.UserIdFk == userId && x.IsActive == true).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList());

            selectedUserRoleAccessIds.RemoveAll(x => _userAccess.Table.Where(x => x.IsActive == false && x.UserIdFk == userId && x.RoleIdFk == roleId).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList().Contains(x));

            if (treeViewItems.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = new { TreeViewItems = treeViewItems, SelectedIds = selectedUserRoleAccessIds.Distinct() },
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Data not Found",
                    Body = null,
                };
            }
            return response;
        }

        public BaseResponse GetComponentsByUserRoleId(int roleId, int userId)
        {
            BaseResponse response = null;

            var RCAresultData = _dbContext.LoadStoredProcedure("md_getComponentAccessByUserAndRole")
             .WithSqlParam("@pUserId", userId)
             .WithSqlParam("@pRoleId", roleId)
             .ExecuteStoredProc<ComponentAccessByRoleAndUserVM>();

            var treeItems = RCAresultData.Select(x => new ComponentAccessByRoleAndUserTreeVM()
            {
                ComponentId = x.ComponentId.ToString(),
                ParentComponentId = x.ParentComponentId,
                ModuleName = x.ComModuleName,
                IsAction = x.IsAction
            }).ToList();
            var treeViewItems = treeItems.BuildComponentAccessTree();
            if (RCAresultData.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = treeViewItems
                };
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Data not Found",
                    Body = null,
                };
            }
            return response;
        }

        public BaseResponse AddOrUpdateUserRoleComponentAccess(ComponentAccessUserRoleVMUpdate componentAccess)
        {
            if (componentAccess.RoleId > 0 && componentAccess.UserId > 0)
            {
                /////////// Get Allowed Component Ids /////////
                var compIds = componentAccess.AccessAttr.Select(x => Convert.ToInt32(x.Key)).Distinct().ToList();

                /////////// Get Allowed Role Access /////////
                var roleAccess = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && x.IsActive == true).ToList();

                /////////// Get Matched Access Ids /////////
                var MatcheAccessIds = roleAccess.Where(x => compIds.Contains(x.ComponentIdFk)).Select(x => x.ComponentIdFk).ToList();

                /////////// Delete those components which are now allowed and Already Exist in Component Access and also Exist in User Access with isActive = false /////////
                var dltFromUserComps = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId &&
                                        x.UserIdFk == componentAccess.UserId && compIds.Contains(x.ComponentIdFk) &&
                                        x.IsActive == false).ToList();

                _userAccess.DeleteRange(dltFromUserComps);

                ///////////// Remove Matched Role Components from coming List /////////
                compIds.RemoveAll(x => MatcheAccessIds.Contains(x));

                /////////// Get AlreadyExist Allowed UserRole Access /////////
                var alreadyExist = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && x.UserIdFk == componentAccess.UserId && x.IsActive == true).ToList();

                ///////////// Remove Access from components /////////
                var removeCompsAccess = alreadyExist.Where(x => !compIds.Contains(x.ComponentIdFk) && x.IsActive == true).ToList();
                removeCompsAccess.ForEach(x => x.IsActive = false);
                _userAccess.Update(removeCompsAccess);

                ///////////// Remove Already Exist User Component Access from coming List /////////
                compIds.RemoveAll(x => alreadyExist.Select(x => x.ComponentIdFk).Contains(x));

                ///////////// Remove NotAllowed User Component Access from coming List /////////
                compIds.RemoveAll(x => removeCompsAccess.Select(x => x.ComponentIdFk).Contains(x));

                /////////// Now add new ones /////////
                var comps = compIds.Select(x => new UserAccess()
                {

                    ComponentIdFk = x,
                    RoleIdFk = componentAccess.RoleId,
                    UserIdFk = componentAccess.UserId,
                    IsActive = true,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _userAccess.Insert(comps);

                /////////// Get Components which Allowed to Role but NotAllowed to User /////////
                var UnMatcheAccessIds = roleAccess.Where(x => !componentAccess.AccessAttr.Select(x => Convert.ToInt32(x.Key)).Distinct().ToList().Contains(x.ComponentIdFk)).Select(x => x.ComponentIdFk).ToList();
                UnMatcheAccessIds.RemoveAll(x => _userAccess.Table.Where(x => x.IsActive == false && x.UserIdFk == componentAccess.UserId && x.UserIdFk == componentAccess.RoleId).Select(x => x.ComponentIdFk).Contains(x));
                if (UnMatcheAccessIds.Count() > 0)
                {
                    /////////// Now add NotAllowed ones /////////
                    comps = UnMatcheAccessIds.Select(x => new UserAccess()
                    {

                        ComponentIdFk = x,
                        RoleIdFk = componentAccess.RoleId,
                        UserIdFk = componentAccess.UserId,
                        IsActive = false,
                        CreatedBy = componentAccess.LoggedInUserId,
                        CreatedDate = DateTime.UtcNow,
                    }).ToList();

                    _userAccess.Insert(comps);
                }

                /////////// Now Delete Allowed Components from UserAccess Which is already Exist in ComponentAccess  /////////
                var duplicateComps = _userAccess.Table.Where(x => roleAccess.Select(x => x.ComponentIdFk).Contains(x.ComponentIdFk) && x.RoleIdFk == componentAccess.RoleId && x.UserIdFk == componentAccess.UserId && x.IsActive == true).AsQueryable().ToList();
                _userAccess.DeleteRange(duplicateComps);

                var extras = _userAccess.Table.Where(x => x.IsActive == false && x.UserIdFk == componentAccess.UserId && x.RoleIdFk == componentAccess.RoleId && !_componentAccess.Table.Where(r => r.RoleIdFk == componentAccess.RoleId).Select(x => x.ComponentIdFk).Contains(x.ComponentIdFk)).ToList();
                _userAccess.DeleteRange(extras);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Component Access saved successfully" };

            }
            else if (componentAccess.RoleId > 0)
            {
                /////////// Get Allowed Component Ids /////////
                var compIds = componentAccess.AccessAttr.Select(x => Convert.ToInt32(x.Key)).Distinct().ToList();

                /////////// Get List of that components from which Access is Removed /////////
                var toBeDeleteComps = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && !compIds.Contains(x.ComponentIdFk)).ToList();

                /////////// Remove List of that components from which Access is Removed /////////
                toBeDeleteComps.ForEach(x => { x.IsActive = false; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _componentAccess.Update(toBeDeleteComps);

                /////////// Get Already Exist Component For the Selected Role /////////
                var alreadyExistComps = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId).ToList();

                /////////// Get Already Exist Active Component Ids /////////
                var alreadyExistActiveCompIds = alreadyExistComps.Where(x => x.IsActive == true).Select(x => x.ComponentIdFk).ToList();

                /////////// Remove Already Exist Component from coming List /////////
                compIds.RemoveAll(x => alreadyExistActiveCompIds.Contains(x));

                /////////// Get Already Exist DeActive Component from Db /////////
                var compsNeedToBeActive = alreadyExistComps.Where(x => x.IsActive == false && compIds.Contains(x.ComponentIdFk)).ToList();

                /////////// Activate and Update Already Exist DeActive Component from Db /////////
                compsNeedToBeActive.ForEach(x => { x.IsActive = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _componentAccess.Update(compsNeedToBeActive);

                /////////// Remove Already Exist Updated Component from coming List /////////
                compIds.RemoveAll(x => compsNeedToBeActive.Select(x => x.ComponentIdFk).Contains(x));

                /////////// Now add new ones /////////
                var comps = compIds.Select(x => new ComponentAccess()
                {

                    ComponentIdFk = x,
                    RoleIdFk = componentAccess.RoleId,
                    IsActive = true,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _componentAccess.Insert(comps);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Access saved successfully" };
            }

            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Please select a role." };
        }

        #endregion

        #region Control List and Details

        public BaseResponse GetUCLDetails(List<int> Ids)
        {
            var UCLDetails = (from ucl in _controlList.Table
                              join ucld in _controlListDetails.Table on ucl.ControlListId equals ucld.ControlListIdFk
                              where Ids.Contains(ucl.ControlListId) && ucl.IsDeleted == false && ucld.IsDeleted == false
                              && ucl.ControlListIsActive == true && ucld.IsActive == true
                              select new
                              {
                                  ParentId = ucl.ControlListId,
                                  ParetntTitle = ucl.ControlListTitle,
                                  Id = ucld.ControlListDetailId,
                                  Title = ucld.Title,
                                  Description = ucld.Description,
                                  ImageHtml = ucld.ImageHtml
                              }).ToList();

            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            foreach (var item in Ids)
            {
                keyValues.Add(UCLDetails.Where(x => x.ParentId == item).Select(x => x.ParetntTitle).FirstOrDefault(), UCLDetails.Where(x => x.ParentId == item).ToList());
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = keyValues };
        }


        #endregion


    }
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AdminService : IAdminService
    {
        private IRepository<Component> _component;
        private IRepository<Role> _role;
        private IRepository<User> _user;
        private IRepository<ComponentAccess> _componentAccess;
        private IRepository<UserAccess> _userAccess;
        private IRepository<UserRole> _userRole;
        IConfiguration _config;
        public AdminService(IConfiguration config,
            IRepository<Component> component,
            IRepository<ComponentAccess> componentAccess,
            IRepository<User> user,
            IRepository<Role> role,
            IRepository<UserRole> userRole,
            IRepository<UserAccess> userAccess)
        {
            this._componentAccess = componentAccess;
            this._component = component;
            this._config = config;
            this._component = component;
            this._user = user;
            this._role = role;
            this._userRole = userRole;
            this._userAccess = userAccess;
        }


        #region Users

        public BaseResponse GetAllUsers()
        {
            var result = _user.Table.Where(x => x.IsDeleted == false).ToList();
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = result };
        }

        public BaseResponse GetUserById(int Id)
        {
            var USER = _user.Table.Where(x => x.UserId == Id && x.IsDeleted == false).FirstOrDefault();
            if (USER != null)
            {
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User Found", Body = USER };
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
                _user.Update(USER);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User Deleted" };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "User Not Found" };
            }

        }


        #endregion



        #region Roles
        public IQueryable<Role> getRoleList()
        {
            return this._role.GetList().Where(item => !item.IsDeleted);
        }
        public IQueryable<UserRoleVM> getRoleListByUserId(int UserId)
        {
            var userRoleList = this._userRole.GetList().Where(item => item.UserIdFK == UserId);
            var roleList = this._role.GetList().Where(item => !item.IsDeleted);
            var userRoles = ( from ur in userRoleList
                              join r in roleList
                              on ur.RoleIdFK equals r.RoleId
                              select new UserRoleVM
                              {
                                  //UserRoleId = ur.UserRoleId,
                                  //UserId = ur.UserIdFK,
                                  RoleId = ur.RoleIdFK,
                                  RoleName = r.RoleName
                              }
                              );
            return userRoles;
        }
        public string SaveRole(RoleVM role)
        {
            string response = string.Empty;
            var newRole = AutoMapperHelper.MapSingleRow<RoleVM, Role>(role);
            if (role.RoleId > 0)
            {
                if (_role.Table.Count(r => r.RoleName == role.RoleName && !r.IsDeleted) == 0)
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
                _role.Update(newRole);
                response = StatusEnums.Success.ToString();
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

        public BaseResponse GetComponentsByRoleId(int Id)
        {
            var response = new BaseResponse();
            //var roleAccess = _componentAccess.Table.Where(x => x.RoleIdFk == Id.ToString() && x.IsDeleted == false).ToList();
            //var compIds = roleAccess.Select(x => x.ComIdFk).ToList();
            //var roleComponents = _component.Table.Where(x => compIds.Contains(x.ComponentId) && x.IsDeleted == false ).ToList();
            //var roleAccessVM = new List<ComponentAccessVM>();
            //roleAccessVM = AutoMapperHelper.MapList<ComponentAccess, ComponentAccessVM>(roleAccess);
            //foreach (var item in roleAccessVM)
            //{
            //    item.Component = new ComponentVM();
            //    var roleComponentsVM = roleComponents.Where(x => x.ComponentId == item.ComIdFk).FirstOrDefault();
            //    if (roleComponentsVM != null) 
            //    {
            //        item.Component = AutoMapperHelper.MapSingleRow<Component, ComponentVM>(roleComponentsVM);
            //    }
            //}


            //<--------Make a join of ComponentAccess Table with Component to get List of Accessible Components By Role Id----------->
            var roleAcceess = (from rca in _componentAccess.Table
                               join ra in _component.Table on rca.ComIdFk equals ra.ComponentId
                               where rca.RoleIdFk == Id.ToString() && ra.IsDeleted == false && rca.IsDeleted == false
                               select new ComponentAccessVM()
                               {
                                   id = ra.ComponentId,
                                   text = ra.ComModuleName,
                                   parent = ra.ParentComponentId != null ? ra.ParentComponentId.ToString() : "#",
                                   state = new ComponentAccessStateVM() { opened = true },
                               }).ToList();

            if (roleAcceess.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = roleAcceess,
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
            var response = new BaseResponse();

            //////// Make a join of ComponentAccess Table with Component to get List of Accessible Components By Role Id ////////
            var userRoleAcceess = (from rca in _componentAccess.Table
                                   join ra in _component.Table on rca.ComIdFk equals ra.ComponentId
                                   join uca in _userAccess.Table on ra.ComponentId equals uca.UserComIdFk
                                   where rca.RoleIdFk == roleId.ToString()
                                   && uca.RoleIdFk == roleId && uca.UserIdFk == userId
                                   && ra.IsDeleted == false && rca.IsDeleted == false && uca.IsDeleted == false
                                   select new ComponentAccessVM()
                                   {
                                       id = ra.ComponentId,
                                       text = ra.ComModuleName,
                                       parent = ra.ParentComponentId != null ? ra.ParentComponentId.ToString() : "#",
                                       state = new ComponentAccessStateVM() { opened = true },
                                   }).ToList();

            if (userRoleAcceess.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = userRoleAcceess,
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


        public BaseResponse AddOrUpdateUserRoleComponentAccess(ComponentAccessUserRoleVM componentAccess)
        {
            if (componentAccess.RoleId > 0 && componentAccess.UserId > 0)
            {
                /////////// Get Allowed Component Ids /////////
                var compIds = componentAccess.Attributes.Where(x => x.selected == true).Select(x => x.id).ToList();

                /////////// Get Allowed Role Access /////////
                var roleAccess = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId.ToString() && x.IsDeleted == false).ToList();

                /////////// Get Matched Access Ids /////////
                var MatcheAccessIds = roleAccess.Where(x => compIds.Contains(x.ComIdFk.Value)).Select(x => x.ComIdFk).ToList();

                ///////////// Remove Matched Role Components from coming List /////////
                compIds.RemoveAll(x => MatcheAccessIds.Contains(x));

                /////////// Get AlreadyExist Allowed UserRole Access /////////
                var alreadyExist = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && x.UserIdFk == componentAccess.UserId && x.IsDeleted == false).ToList();

                ///////////// Remove Already Exist User Component Access from coming List /////////
                compIds.RemoveAll(x => alreadyExist.Select(x => x.UserComIdFk).Contains(x));

                var extrasCompsAccess = alreadyExist.Where(x => !compIds.Contains(x.UserComIdFk) && x.IsDeleted == false).ToList();

                extrasCompsAccess.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _userAccess.Update(extrasCompsAccess);

                /////////// Now add new ones /////////
                var comps = compIds.Select(x => new UserAccess()
                {

                    UserComIdFk = x,
                    RoleIdFk = componentAccess.RoleId,
                    UserIdFk = componentAccess.UserId,
                    UserActive = true,
                    IsDeleted = false,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _userAccess.Insert(comps);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Component Access saved successfully" };

                ///////////// Get List of that components from which Access is Removed /////////
                //var toBeDeleteComps = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId.ToString() && x.UserId == componentAccess.UserId && !compIds.Contains(x.UserComIdFk)).ToList();

                ///////////// Remove List of that components from which Access is Removed /////////
                //toBeDeleteComps.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                //_userAccess.Update(toBeDeleteComps);

                ///////////// Get Already Exist Component For the Selected Role and User /////////
                //var alreadyExistComps = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId.ToString() && x.UserId == componentAccess.UserId).ToList();

                ///////////// Get Already Exist Active Component Ids /////////
                //var alreadyExistActiveCompIds = alreadyExistComps.Where(x => x.UserActive == true).Select(x => x.UserComIdFk).ToList();

                ///////////// Remove Already Exist Component from coming List /////////
                //compIds.RemoveAll(x => alreadyExistActiveCompIds.Contains(x));

                ///////////// Get Already Exist DeActive Component from Db /////////
                //var compsNeedToBeActive = alreadyExistComps.Where(x => x.UserActive == false && compIds.Contains(x.UserComIdFk)).ToList();

                ///////////// Activate and Update Already Exist DeActive Component from Db /////////
                //compsNeedToBeActive.ForEach(x => { x.UserActive = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                //_userAccess.Update(compsNeedToBeActive);

                ///////////// Remove Already Exist Updated Component from coming List /////////
                //compIds.RemoveAll(x => compsNeedToBeActive.Select(x => x.UserComIdFk).Contains(x));

            }
            else if (componentAccess.RoleId > 0)
            {
                /////////// Get Allowed Component Ids /////////
                var compIds = componentAccess.Attributes.Where(x => x.selected == true).Select(x => x.id).ToList();

                /////////// Get List of that components from which Access is Removed /////////
                var toBeDeleteComps = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId.ToString() && !compIds.Contains(x.ComIdFk.Value)).ToList();

                /////////// Remove List of that components from which Access is Removed /////////
                toBeDeleteComps.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _componentAccess.Update(toBeDeleteComps);

                /////////// Get Already Exist Component For the Selected Role /////////
                var alreadyExistComps = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId.ToString()).ToList();

                /////////// Get Already Exist Active Component Ids /////////
                var alreadyExistActiveCompIds = alreadyExistComps.Where(x => x.Active == true).Select(x => x.ComIdFk).ToList();

                /////////// Remove Already Exist Component from coming List /////////
                compIds.RemoveAll(x => alreadyExistActiveCompIds.Contains(x));

                /////////// Get Already Exist DeActive Component from Db /////////
                var compsNeedToBeActive = alreadyExistComps.Where(x => x.Active == false && compIds.Contains(x.ComIdFk.Value)).ToList();

                /////////// Activate and Update Already Exist DeActive Component from Db /////////
                compsNeedToBeActive.ForEach(x => { x.Active = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _componentAccess.Update(compsNeedToBeActive);

                /////////// Remove Already Exist Updated Component from coming List /////////
                compIds.RemoveAll(x => compsNeedToBeActive.Select(x => x.ComIdFk).Contains(x));

                /////////// Now add new ones /////////
                var comps = compIds.Select(x => new ComponentAccess()
                {

                    ComIdFk = x,
                    RoleIdFk = componentAccess.RoleId.ToString(),
                    Active = true,
                    IsDeleted = false,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _componentAccess.Insert(comps);
                return new BaseResponse() { Status = System.Net.HttpStatusCode.OK, Message = "Access saved successfully" };
            }

            return new BaseResponse() { Status = System.Net.HttpStatusCode.NotFound, Message = "Please select a role." };
        }

        #endregion

    }
}

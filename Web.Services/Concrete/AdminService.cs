using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Web.API.Helper;
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
        private RAQ_DbContext _dbContext;
        private IRepository<Component> _component;
        private IRepository<Role> _role;
        private IRepository<User> _user;
        private IRepository<ComponentAccess> _componentAccess;
        private IRepository<UserAccess> _userAccess;
        private IRepository<UserRole> _userRole;
        IConfiguration _config;
        public AdminService(RAQ_DbContext dbContext,
            IConfiguration config,
            IRepository<Component> component,
            IRepository<ComponentAccess> componentAccess,
            IRepository<User> user,
            IRepository<Role> role,
            IRepository<UserRole> userRole,
            IRepository<UserAccess> userAccess)
        {
            this._dbContext = dbContext;
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
            var users = AutoMapperHelper.MapList<User, RegisterCredentialVM>(result);
            foreach (var item in users)
            {
                item.UserRole = getRoleListByUserId(item.UserId).ToList();
            }
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Users List Returned", Body = users };
        }

        public BaseResponse GetUserById(int Id)
        {
            var USER = _user.Table.Where(x => x.UserId == Id && x.IsDeleted == false).FirstOrDefault();
            if (USER != null)
            {
                var user = AutoMapperHelper.MapSingleRow<User, RegisterCredentialVM>(USER);
                user.UserRole = getRoleListByUserId(Id).ToList();
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
                             where ur.RoleIdFk == roleId
                             select u
                             ).ToList();
            var roleUsersVM = AutoMapperHelper.MapList<User, RegisterCredentialVM>(roleUsers);
            var response = new BaseResponse();

            if (roleUsersVM.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = roleUsersVM,
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

        #endregion

        #region Roles
        public IQueryable<Role> getRoleList()
        {
            return this._role.GetList().Where(item => !item.IsDeleted);
        }
        public IQueryable<UserRoleVM> getRoleListByUserId(int UserId)
        {
            var userRoleList = this._userRole.GetList().Where(item => item.UserIdFk == UserId);
            var roleList = this._role.GetList().Where(item => !item.IsDeleted);
            var userRoles = (from ur in userRoleList
                             join r in roleList
                             on ur.RoleIdFk equals r.RoleId
                             select new UserRoleVM
                             {
                                 //UserRoleId = ur.UserRoleId,
                                 //UserId = ur.UserIdFK,
                                 RoleId = ur.RoleIdFk,
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
                expanded = true,
            }).ToList();
            var treeViewItems = treeItems.BuildTree();
            var selectedRoleAccessIds = _componentAccess.Table.Where(x => x.RoleIdFk == Id && x.IsDeleted == false).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList();

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
                expanded = true,
            }).ToList();
            var treeViewItems = treeItems.BuildTree();
            //<--------Make a join of ComponentAccess Table with Component to get List of Accessible Components By Role and user Id----------->
            var selectedUserRoleAccessIds = _componentAccess.Table.Where(x => x.RoleIdFk == roleId && x.IsActive == true && x.IsDeleted == false).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList();
            selectedUserRoleAccessIds.AddRange(_userAccess.Table.Where(x => x.RoleIdFk == roleId && x.UserIdFk == userId && x.IsActive == true && x.IsDeleted == false).Select(x => new { key = x.ComponentIdFk.ToString() }).ToList());

            if (treeViewItems.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = new { TreeViewItems = treeViewItems, SelectedIds = selectedUserRoleAccessIds },
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
            List<ComponentAccessVM> roleacceess = (from ca in _componentAccess.Table
                               join c in _component.Table on ca.ComponentIdFk equals c.ComponentId
                               where ca.RoleIdFk == roleId && !c.IsDeleted && !ca.IsDeleted && ca.IsActive
                               select new ComponentAccessVM()
                               {
                                   ComponentId = c.ComponentId,
                                   ComModuleName = c.ComModuleName,
                                   RoleId = ca.RoleIdFk,
                                   ParentComponentId = c.ParentComponentId,
                                   
                               }).ToList();
            //var queryReturn = this._dbContext.UserRoles. .ExcuteSql("select * from Roles");
            //List<ComponentAccessVM> roleaccees = this. //dasq("CreateStudents @p0, @p1", parameters: new[] { "Bill", "Gates" });
            //var userRoleAcceess = (from rca in _componentAccess.Table
            //                       join ra in _component.Table on rca.ComIdFk equals ra.ComponentId
            //                       join uca in _userAccess.Table on ra.ComponentId equals uca.UserComIdFk
            //                       where rca.RoleIdFk == roleId
            //                       && uca.RoleIdFk == roleId && uca.UserIdFk == userId
            //                       && ra.IsDeleted == false && rca.IsDeleted == false && uca.IsDeleted == false
            //                       select new ComponentAccessVM()
            //                       {
            //                           id = ra.ComponentId,
            //                           text = ra.ComModuleName,
            //                           parent = ra.ParentComponentId != null ? ra.ParentComponentId.ToString() : "#",
            //                           state = new ComponentAccessStateVM() { opened = true },
            //                       }).ToList();


            if (roleacceess.Count() > 0)
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.OK,
                    Message = "Data Found",
                    Body = roleacceess 
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
                var roleAccess = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && x.IsDeleted == false && x.IsActive == true).ToList();

                /////////// Get Matched Access Ids /////////
                var MatcheAccessIds = roleAccess.Where(x => compIds.Contains(x.ComponentIdFk)).Select(x => x.ComponentIdFk).ToList();

                ///////////// Remove Matched Role Components from coming List /////////
                compIds.RemoveAll(x => MatcheAccessIds.Contains(x));

                /////////// Get AlreadyExist Allowed UserRole Access /////////
                var alreadyExist = _userAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && x.UserIdFk == componentAccess.UserId && x.IsActive == true && x.IsDeleted == false).ToList();

                ///////////// Remove Already Exist User Component Access from coming List /////////
                compIds.RemoveAll(x => alreadyExist.Select(x => x.ComponentIdFk).Contains(x));

                ///////////// Remove Access from components /////////
                var removeCompsAccess = alreadyExist.Where(x => !compIds.Contains(x.ComponentIdFk) && x.IsDeleted == false && x.IsActive == true).ToList();
                removeCompsAccess.ForEach(x => x.IsActive = false);
                _userAccess.Update(removeCompsAccess);

                ///////////// Remove components which has no access/////////
                var extrasCompsAccess = alreadyExist.Where(x => !compIds.Contains(x.ComponentIdFk) && x.IsDeleted == false && x.IsActive == false).ToList();
                extrasCompsAccess.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
                _userAccess.Update(extrasCompsAccess);

                /////////// Now add new ones /////////
                var comps = compIds.Select(x => new UserAccess()
                {

                    ComponentIdFk = x,
                    RoleIdFk = componentAccess.RoleId,
                    UserIdFk = componentAccess.UserId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _userAccess.Insert(comps);

                /////////// Get Components which Allowed to Role but NotAllowed to User /////////
                var UnMatcheAccessIds = roleAccess.Where(x => !compIds.Contains(x.ComponentIdFk)).Select(x => x.ComponentIdFk).ToList();

                /////////// Now add NotAllowed ones /////////
                comps = compIds.Select(x => new UserAccess()
                {

                    ComponentIdFk = x,
                    RoleIdFk = componentAccess.RoleId,
                    UserIdFk = componentAccess.UserId,
                    IsActive = false,
                    IsDeleted = false,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _userAccess.Insert(comps);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Component Access saved successfully" };

            }
            else if (componentAccess.RoleId > 0)
            {
                /////////// Get Allowed Component Ids /////////
                var compIds = componentAccess.AccessAttr.Select(x => Convert.ToInt32(x.Key)).Distinct().ToList();

                /////////// Get List of that components from which Access is Removed /////////
                var toBeDeleteComps = _componentAccess.Table.Where(x => x.RoleIdFk == componentAccess.RoleId && !compIds.Contains(x.ComponentIdFk)).ToList();

                /////////// Remove List of that components from which Access is Removed /////////
                toBeDeleteComps.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = componentAccess.LoggedInUserId; x.ModifiedDate = DateTime.UtcNow; });
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
                    IsDeleted = false,
                    CreatedBy = componentAccess.LoggedInUserId,
                    CreatedDate = DateTime.UtcNow,
                }).ToList();

                _componentAccess.Insert(comps);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Access saved successfully" };
            }

            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Please select a role." };
        }



        #endregion

    }
}

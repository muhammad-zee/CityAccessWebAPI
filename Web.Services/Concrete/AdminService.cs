using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class AdminService : IAdminService
    {
        private IRepository<Component> _component;
        private IRepository<Role> _role;
        private IRepository<ComponentAccess> _componentAccess;
        IConfiguration _config;
        public AdminService(IConfiguration config, 
            IRepository<Component> component, 
            IRepository<ComponentAccess> componentAccess, 
            IRepository<Role> role)
        {
            this._componentAccess = componentAccess;
            this._component = component;
            this._config = config;
            this._component = component;
            this._role = role;
        }
        #region Roles

        public IQueryable<Role> getRoleList()
        {
            return this._role.GetList().Where(item => !item.IsDeleted);
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
                Message = UserEnums.Created.ToString(),
                Body = _component.GetList().ToList(),
            };

            return response;
        }

        #endregion

        #region Component Access

        public BaseResponse GetAllComponents()
        {
            var response = new BaseResponse();
            try
            {
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

            }
            catch (Exception ex)
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }

            return response;
        }

        public BaseResponse GetComponentById(int Id)
        {
            var response = new BaseResponse();
            try
            {
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

            }
            catch (Exception ex)
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }

            return response;
        }

        public BaseResponse GetComponentsByRoleId(int Id)
        {
            var response = new BaseResponse();
            try
            {
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
                        Status = System.Net.HttpStatusCode.OK,
                        Message = "Data Found",
                        Body = roleAcceess,
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

            }
            catch (Exception ex)
            {
                response = new BaseResponse()
                {
                    Status = System.Net.HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }

            return response;
        }


        public BaseResponse AddOrUpdateUserRoleComponentAccess()
        {

            return new BaseResponse();
        }

        #endregion

    }
}

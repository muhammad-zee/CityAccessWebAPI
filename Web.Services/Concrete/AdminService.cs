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
    public class AdminService: IAdminService
    {
        private IRepository<Component> _component;
        IConfiguration _config;
        public AdminService(IConfiguration config, IRepository<Component> component)
        {
            this._component = component;
            this._config = config;
        }

        #region Component

        public BaseResponse AddOrUpdateComponent(List<ComponentVM> components) 
        {
            var comp_Controllers = components.Select(x => x.ComModuleName).Distinct().ToList();
            var comp_Action = components.Select(x => new { x.ComModuleName, x.PageName, x.PageDescription}).ToList();

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

            var extras = _component.Table.Where(x => !(controllers.Contains(x.ComModuleName) && x.ParentComponentId == null) && !(Action.Contains(x.ComModuleName) && x.ParentComponentId != null )).ToList();
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
    }
}

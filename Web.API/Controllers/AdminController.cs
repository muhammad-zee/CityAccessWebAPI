using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Interfaces;


namespace Web.API.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public AdminController(IAdminService adminService, IConfiguration config, IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment);
        }


        #region DashBoard

        [Description("Get Label Counts")]
        [HttpGet("admin/GetLabelCounts")]
        public BaseResponse GetLabelCounts()
        {
            try
            {
                return _adminService.GetLabelCounts();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #endregion

        #region Users

        [Description("Get Users List")]
        [HttpGet("admin/GetAllUsers")]
        public async Task<BaseResponse> GetAllUsers(int? OrganizationId, int? UserRoleId)
        {
            try
            {
                BaseResponse response = null;
                if (OrganizationId != null)
                {
                    response = _adminService.GetAllUsersByOrganizationId(OrganizationId.Value, UserRoleId.Value);

                }
                else
                {
                    response = _adminService.GetAllUsers();
                }

                return response;

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Users List")]
        [HttpGet("admin/GetAllUsersByServiceAndRoleId")]
        public async Task<BaseResponse> GetAllUsersByServiceAndRoleId(int ServiceLineId, int RoleId)
        {
            try
            {
                BaseResponse response = null;
                response = _adminService.GetAllUsersByServiceLineAndRoleId(ServiceLineId, RoleId);
                return response;

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get User By Id")]
        [HttpGet("admin/GetUserById/{Id}")]
        public async Task<BaseResponse> GetUserById(int Id)
        {
            try
            {
                var response = _adminService.GetUserById(Id);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete User")]
        [HttpGet("admin/DeleteUser/{Id}")]
        public async Task<BaseResponse> DeleteUser(int Id)
        {
            try
            {
                var response = _adminService.DeleteUser(Id);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        #endregion

        #region User Role

        [HttpGet("admin/GetUsersByRoleId/{roleId}")]
        public async Task<BaseResponse> GetUsersByRoleId(int roleId)
        {
            try
            {
                var response = new BaseResponse();
                response = _adminService.GetUsersByRoleId(roleId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }

        }


        #endregion

        #region Role

        // GET: api/<EmployeeController>
        [HttpGet("admin/GetAllRoles")]
        public BaseResponse GetRoles(int? OrganizationID, int? userRoleId)
        {
            try
            {
                IQueryable roleObj = null;
                if (OrganizationID != null)
                {
                    roleObj = _adminService.getRoleListByOrganizationId(OrganizationID.Value, userRoleId.Value);
                }
                else
                {
                    roleObj = _adminService.getRoleList();
                }
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned", Body = roleObj };

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpGet("admin/getRoleListByOrganizationIds/{Ids}")]
        public BaseResponse GetRoleListByOrganizationIds(string Ids)
        {
            try
            {
                var response = _adminService.getRoleListByOrganizationIds(Ids).ToList();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned", Body = response };

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }
        [HttpGet("admin/GetRolesByUserId/{UserId}")]
        public BaseResponse GetRoles(int UserId)
        {
            try
            {
                var roleObj = _adminService.getRoleListByUserId(UserId).ToList();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned", Body = roleObj };

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpPost("admin/SaveRole")]
        public BaseResponse SaveRole([FromBody] RoleVM role)
        {
            try
            {
                string saveResponse = _adminService.SaveRole(role);
                if (saveResponse == StatusEnums.AlreadyExist.ToString())
                {
                    return new BaseResponse { Status = HttpStatusCode.BadRequest, Message = "Role already exists" };
                }
                else /*if (saveResponse == StatusEnums.Success.ToString())*/
                {
                    return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned" };
                }
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Role")]
        [HttpGet("admin/DeleteRole/{Id}")]
        public async Task<BaseResponse> DeleteRole(int Id)
        {
            try
            {
                var response = _adminService.DeleteRole(Id);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Component

        [Description("Get Controllers Actions List")]
        [HttpPost("admin/getcontrolleractiondetails")]
        public BaseResponse GetControllerActionDetails()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            var controlleractionlist = asm.GetTypes()
                                        .Where(type => typeof(Controller).IsAssignableFrom(type))
                                        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                                        .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any()
                                            && !m.GetCustomAttributes<NonActionAttribute>().Any())
                                        .Select(x => new
                                        {
                                            Controller = x.DeclaringType.Name.Replace("Controller", ""),
                                            Action = x.Name,
                                            ReturnType = x.ReturnType.Name,
                                            Description = x.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault()?.Description ?? String.Empty,
                                            MethodType = String.Join(",", x.GetCustomAttributes().Where(x => x.GetType().Name.Contains("Http")).Select(a => a.GetType().Name.Replace("Attribute", "")))
                                        })
                                        .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

            var ComponentList = new List<ComponentVM>();
            foreach (var item in controlleractionlist)
            {
                ComponentList.Add(new ComponentVM()
                {
                    ComModuleName = item.Controller,
                    PageName = item.Action,
                    //VerbType = item.MethodType,
                    PageDescription = item.Description
                });
            }

            var result = _adminService.AddOrUpdateComponent(ComponentList);

            return result;
        }

        #endregion

        #region Component Access

        [Description("Get All Components")]
        [HttpGet("admin/getallcomponenets")]
        public async Task<BaseResponse> GetAllComponenets()
        {
            try
            {
                var response = new BaseResponse();
                response = _adminService.GetAllComponents();
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Get Component By Id")]
        [HttpGet("admin/getcomponentbyid/{Id}")]
        public async Task<BaseResponse> GetComponentById(int Id)
        {

            try
            {
                var response = new BaseResponse();
                response = _adminService.GetComponentById(Id);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Get All Components By Role Id")]
        [HttpGet("admin/getcomponentsbyroleid/{Id}")]
        public async Task<BaseResponse> GetComponentsByRoleId(int Id)
        {
            try
            {
                var response = new BaseResponse();
                response = _adminService.GetComponentsTreeByRoleId(Id);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Get All Components Tree By UserRole Id")]
        [HttpGet("admin/GetComponentsTreeByUserRoleId")]
        public async Task<BaseResponse> GetComponentsTreeByUserRoleId(int roleId, int userId)
        {
            try
            {
                var response = new BaseResponse();
                response = _adminService.GetComponentsTreeByUserRoleId(roleId, userId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }


        [Description("Get All Components By UserRole Id")]
        [HttpGet("admin/GetComponentsByUserRoleId")]
        public async Task<BaseResponse> GetComponentsByUserRoleId(int roleId, int userId)
        {
            try
            {
                BaseResponse response = null;
                response = _adminService.GetComponentsByUserRoleId(roleId, userId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        //[Description("Add Or Update User Role Component Access")]
        //[HttpPost("admin/upload")]
        //public async Task<BaseResponse> AddOrUpdateUserRoleComponentAccess([FromBody] object componentAccess)
        //{
        //    try
        //    {
        //        var response = new BaseResponse();
        //        //response = _adminService.AddOrUpdateUserRoleComponentAccess(componentAccess);
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        ElmahExtensions.RiseError(ex); _logger.LogExceptions(ex);
        //        return new BaseResponse()
        //        {
        //            Status = HttpStatusCode.BadRequest,
        //            Message = ex.ToString(),
        //            Body = null
        //        };
        //    }
        //}



        [Description("Add/Update User Role Component Access")]
        [HttpPost("admin/UpdateUserRoleComponentAccess")]
        public async Task<BaseResponse> UpdateUserRoleComponentAccess([FromBody] ComponentAccessUserRoleVMUpdate componentAccess)
        {
            try
            {
                var response = new BaseResponse();
                response = _adminService.AddOrUpdateUserRoleComponentAccess(componentAccess);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        #endregion

        #region Control List and Details

        [Description("Get Control List Data")]
        [HttpPost("admin/GetUCLDetails")]
        public BaseResponse GetUCLDetails([FromBody] List<int> Id)
        {
            try
            {
                var result = _adminService.GetUCLDetails(Id);
                return result;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }

        }

        #endregion
    }
}

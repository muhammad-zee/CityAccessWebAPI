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
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    //[Route("Role")]
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

        #region Role

        // GET: api/<EmployeeController>
        [HttpGet("admin/GetAllRoles")]

        public BaseResponse GetRoles()
        {
            try
            {
                var roleObj = _adminService.getRoleList();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned", Body = roleObj };

            }
            catch (Exception ex)
            {
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        //[HttpPost("SaveRole")]
        //public BaseResponse SaveRole(Role)
        //// GET api/<EmployeeController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<EmployeeController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<EmployeeController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<EmployeeController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

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
            var response = new BaseResponse();
            response = _adminService.GetAllComponents();
            return response;
        }

        [Description("Get Component By Id")]
        [HttpGet("admin/getcomponentbyid/{Id}")]
        public async Task<BaseResponse> GetComponentById(int Id)
        {
            var response = new BaseResponse();
            response = _adminService.GetComponentById(Id);
            return response;
        }

        [Description("Get All Components By Role Id")]
        [HttpGet("admin/getcomponentsbyroleid/{Id}")]
        public async Task<BaseResponse> GetComponentsByRoleId(int Id)
        {
            var response = new BaseResponse();
            response = _adminService.GetComponentsByRoleId(Id);
            return response;
        }

        #endregion
    }
}

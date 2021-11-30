using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Data.Models;
using Web.Model;
using Web.Services.Concrete;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [Route("Role")]
    public class RoleController : Controller
    {
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        private readonly IRoleService _roleService;

        public RoleController(IConfiguration config,IWebHostEnvironment environment,IRoleService roleService)
        {
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment);
            this._roleService = roleService;
        }

        // GET: api/<EmployeeController>
        [HttpGet("GetAllRoles")]

        public BaseResponse Get()
        {
            try
            {
                var roleObj = "jhe;laskdjf";// _roleService.getRoleList();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Roles List Returned", Body = roleObj };

            }
            catch (Exception ex)
            {
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

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

    }
}

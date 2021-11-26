using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Data.Models;
using Web.Services.Concrete;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Route("Role")]
    public class RoleController : ControllerBase
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

        public IEnumerable<Role> Get()
        {
            return _roleService.getRoleList();
        }

        // GET api/<EmployeeController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<EmployeeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<EmployeeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EmployeeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}

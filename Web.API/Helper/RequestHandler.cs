using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Web.Services;
using Web.Services.Extensions;
using Web.Services.Helper;

namespace Web.API.Helper
{
    public class RequestHandler : ActionFilterAttribute
    {
        /// <summary>
        /// Get User Info on Every Request from request token
        /// </summary>
        /// <param name="context"></param>

        ////////// Encryption Key //////////////

        private string key = "44 52 d7 16 87 b6 bc 2c 93 89 c3 34 9f dc 17 fb 3d fb ba 62 24 af fb 76 76 e1 33 79 26 cd d6 02";

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            var user = context.HttpContext.User;
            StringValues timeZone;
            StringValues componentObjHEX;

            ///////////////////////// Get Values from Custom Headers and token ///////////////////////////////////////

            context.HttpContext.Request.Headers.TryGetValue("TimeZone", out timeZone);
            context.HttpContext.Request.Headers.TryGetValue("Module-Access-Control", out componentObjHEX);

            string isSuperAdmin = user.Claims.Where(x => x.Type == "isSuperAdmin").Select(x => x.Value).FirstOrDefault();
            string isEMS = user.Claims.Where(x => x.Type == "isEMS").Select(x => x.Value).FirstOrDefault();
            var userAccessObj = user.Claims.Where(x => x.Type == "UserAccess").Select(x => x.Value).FirstOrDefault();
            var userAccess = new List<int>();
            if (userAccessObj != null) 
            {
                userAccess = JsonConvert.DeserializeObject<List<int>>(userAccessObj);
            }
            var userId = Convert.ToInt32(user.Claims.Where(x => x.Type == "UserId").Select(x => x.Value).FirstOrDefault());
            var orgId = Convert.ToInt32(user.Claims.Where(x => x.Type == "organizationId").Select(x => x.Value).FirstOrDefault());
            var userName = user.Claims.Where(x => x.Type.Contains("nameidentifier")).Select(x => x.Value).FirstOrDefault();
            var roleId = user.Claims.Where(x => x.Type == "RoleIds").Select(x => x.Value).FirstOrDefault(); ;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            //////////////////////// Set Values in ApplicationSettings Class ///////////////////////////////////////////

            ApplicationSettings.UserId = userId;
            ApplicationSettings.OrganizationId = orgId;
            ApplicationSettings.UserName = userName;
            ApplicationSettings.RoleIds = roleId;
            ApplicationSettings.isSuperAdmin = isSuperAdmin == "True" ? true : false;
            ApplicationSettings.isEMS = isEMS == "True" ? true : false;
            ApplicationSettings.TimeZone = timeZone.ToString();

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ///////////////////////// Check for Components Access //////////////////////////////////////////////////////

            var authorized = context.Controller.GetType().GetCustomAttributes<AuthorizeAttribute>().Any();
            var componentSerializeObj = Encryption.decryptData(componentObjHEX.ToString(), key);
            if (!ApplicationSettings.isSuperAdmin && authorized)
            {
                if (componentSerializeObj != null)
                {
                    var jobj = JObject.Parse(componentSerializeObj);
                    var componentId = jobj["componentId"].ToString().ToInt();
                    var bypassAccessCheck = jobj["bypassAccessCheck"].ToString().ToBool();
                    if (!bypassAccessCheck)
                    {
                        if (!userAccess.Any(x => x == componentId))
                        {
                            context.HttpContext.Response.StatusCode = 401;
                            context.Result = new UnauthorizedResult();
                        }
                    }
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 401;
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                base.OnActionExecuting(context);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

    }
}

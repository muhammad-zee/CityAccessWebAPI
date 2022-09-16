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
            var userIP = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var user = context.HttpContext.User;
            ///////////////////////// Get Values from Custom Headers and token ///////////////////////////////////////


            var userId = Convert.ToInt32(user.Claims.Where(x => x.Type == "UserId").Select(x => x.Value).FirstOrDefault());
            var UserFullName = Convert.ToString(user.Claims.Where(x => x.Type == "UserFullName").Select(x => x.Value).FirstOrDefault());
            var partnerId = Convert.ToInt32(user.Claims.Where(x => x.Type == "PartnerId").Select(x => x.Value).FirstOrDefault());
            var partnerTradeName = Convert.ToString(user.Claims.Where(x => x.Type == "PartnerTradeName").Select(x => x.Value).FirstOrDefault());
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            //////////////////////// Set Values in ApplicationSettings Class ///////////////////////////////////////////

            ApplicationSettings.UserId = userId;
            ApplicationSettings.UserFullName = UserFullName;
            ApplicationSettings.PartnerId = partnerId;
            ApplicationSettings.PartnerTradeName = partnerTradeName;


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ///////////////////////// Check for Components Access //////////////////////////////////////////////////////

            //var controllerAuthorized = context.Controller.GetType().GetCustomAttributes<AuthorizeAttribute>().Any();
            //var methodAuthorized = context.ActionDescriptor.GetType().GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            //var componentSerializeObj = Encryption.decryptData(componentObjHEX.ToString(), key);
            //if (!ApplicationSettings.isSuperAdmin && controllerAuthorized)
            //{
            //    if (!methodAuthorized)
            //    {
            //        base.OnActionExecuting(context);
            //    }
            //    else
            //    {
            //        if (componentSerializeObj != null)
            //        {
            //            var jobj = JObject.Parse(componentSerializeObj);
            //            var componentId = jobj["componentId"].ToString().ToInt();
            //            var bypassAccessCheck = jobj["bypassAccessCheck"].ToString().ToBool();
            //            if (!bypassAccessCheck)
            //            {
            //                if (!userAccess.Any(x => x == componentId))
            //                {
            //                    context.HttpContext.Response.StatusCode = 401;
            //                    context.Result = new UnauthorizedResult();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            context.HttpContext.Response.StatusCode = 401;
            //            context.Result = new UnauthorizedResult();
            //        }
            //    }

            //}
            //else
            //{
            //    base.OnActionExecuting(context);
            //}

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

    }

}

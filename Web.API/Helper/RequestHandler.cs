﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using Web.Services;

namespace Web.API.Helper
{
    public class RequestHandler : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            StringValues timeZone;
            context.HttpContext.Request.Headers.TryGetValue("TimeZone", out timeZone);

            string isSuperAdmin = user.Claims.Where(x => x.Type == "isSuperAdmin").Select(x => x.Value).FirstOrDefault();

            ApplicationSettings.UserId = Convert.ToInt32(user.Claims.Where(x => x.Type == "UserId").Select(x => x.Value).FirstOrDefault());
            ApplicationSettings.UserName = user.Claims.Where(x => x.Type.Contains("nameidentifier")).Select(x => x.Value).FirstOrDefault();
            ApplicationSettings.RoleIds = user.Claims.Where(x => x.Type == "RoleIds").Select(x => x.Value).FirstOrDefault();
            ApplicationSettings.isSuperAdmin = isSuperAdmin == "True" ? true : false;
            ApplicationSettings.TimeZone = timeZone.ToString();

            base.OnActionExecuting(context);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;
using Web.Data.Models;
using System.Dynamic;
using Web.Services.Concrete;
using Newtonsoft.Json.Linq;

namespace Web.Services.Helper
{
    public static class ActivityLogService
    {

        public static void Log<T>(this DbContext _dbContext, T? items, string tableName, int tablePrimayrKey, int action)
        {
            var json = JsonSerializer.Serialize(items);
            string description = generateLogDesc(tableName, action, json);

            int rowsAffected;
            string sql = "EXEC md_saveActivityLog " +
                "@pUserIdFk, " +
                "@pTableName, " +
                "@pTablePrimaryKey, " +
                "@pAction, " +
                "@pChangeset," +
                "@pDescription";
            int userId = action == ActivityLogActionEnums.SignIn.ToInt() ? tablePrimayrKey : ApplicationSettings.UserId;
            List<SqlParameter> parms = new List<SqlParameter>
                    { 
                        // Create parameters    
                        new SqlParameter { ParameterName = "@pUserIdFk", Value = userId},
                        new SqlParameter { ParameterName = "@pTableName", Value = tableName },
                        new SqlParameter { ParameterName = "@pTablePrimaryKey", Value =tablePrimayrKey },
                        new SqlParameter { ParameterName = "@pAction", Value = action },
                        new SqlParameter { ParameterName = "@pChangeset", Value = json,Size=int.MaxValue},
                        new SqlParameter { ParameterName = "@pDescription", Value = description,Size=int.MaxValue }
                    };

            rowsAffected = _dbContext.Database.ExecuteSqlRaw(sql, parms.ToArray());

        }

        public static string generateLogDesc(string tableName, int action, string jsonString)
        {
            var jobj = JObject.Parse(jsonString);
            string logDesc = "";
            //string userFullName = action == ActivityLogActionEnums.SignIn.ToInt() ? jobj["userFullName"].ToString() : ApplicationSettings.UserFullName;
            string userFullName = ApplicationSettings.UserFullName;
            string actionName = action == ActivityLogActionEnums.SignIn.ToInt() ? "Logged In" :
                action == ActivityLogActionEnums.Logout.ToInt() ? "Logged Out" :
                action == ActivityLogActionEnums.Create.ToInt() ? "Created" :
                action == ActivityLogActionEnums.Update.ToInt() ? "Updated" :
                action == ActivityLogActionEnums.Delete.ToInt() ? "Deleted" :
                action == ActivityLogActionEnums.Inactive.ToInt() ? "Deactivated" :
                action == ActivityLogActionEnums.FileUpload.ToInt() ? "Uploaded File" :
                action == ActivityLogActionEnums.FileDelete.ToInt() ? "Deleted File" :
                action == ActivityLogActionEnums.Acknowledge.ToInt() ? "Acknowledged" :
                action == ActivityLogActionEnums.Active.ToInt() ? "Activated" : "Action Performed";

            if (tableName == ActivityLogTableEnums.CodeStrokes.ToString() || tableName == ActivityLogTableEnums.CodeTraumas.ToString() || tableName == ActivityLogTableEnums.CodeBlues.ToString() ||
                tableName == ActivityLogTableEnums.CodeSepsis.ToString() || tableName == ActivityLogTableEnums.CodeSTEMIs.ToString())
            {
                if (action == ActivityLogActionEnums.Update.ToInt())
                {
                    var jobj1 = jobj.Properties().ToList();
                    string updatedField = "";
                    string updatedValue = "";
                    foreach (var i in jobj)
                    {
                        if (i.Value != null)
                        {
                            updatedField = i.Key;
                            updatedValue = i.Value.ToString();
                        }
                    }

                    logDesc = $"{userFullName} {actionName} {updatedField} To {updatedValue} In {tableName}";
                }
                else if (action == ActivityLogActionEnums.FileUpload.ToInt() || action == ActivityLogActionEnums.FileDelete.ToInt())
                {
                    logDesc = $"{userFullName} {actionName} In {tableName}";
                }
                else
                {
                    logDesc = $"{userFullName} {actionName} {tableName}";
                }

            }
            else if (tableName == ActivityLogTableEnums.Consults.ToString())
            {
                if (action == ActivityLogActionEnums.Acknowledge.ToInt())
                {

                }
                logDesc = $"{userFullName} {actionName} Record In {tableName}";

            }
            else
            {
                logDesc = $"{userFullName} {actionName}";
            }

            return logDesc;
        }



    }
}

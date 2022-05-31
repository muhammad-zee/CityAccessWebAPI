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

namespace Web.Services.Helper
{
    public static class ActivityLogService
    {

        public static void Log<T>(this DbContext _dbContext, T? items, string tableName, int tablePrimayrKey, int action)
        {
            var json = JsonSerializer.Serialize(items);
            string description = new SettingsService(null, null, null, null).generateLogDesc(tableName, action, json);

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



    }
}

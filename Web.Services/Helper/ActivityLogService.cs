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

namespace Web.Services.Helper
{
    public static class ActivityLogService 
    {

        public static void Log(this DbContext _dbContext, dynamic items, string tableName, int tablePrimayrKey, int action)
        {
            
            var json = JsonSerializer.Serialize(items);
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
                        new SqlParameter { ParameterName = "@pDescription", Value = "",Size=int.MaxValue }
                    };

            rowsAffected = _dbContext.Database.ExecuteSqlRaw(sql, parms.ToArray());

        }
    }
}

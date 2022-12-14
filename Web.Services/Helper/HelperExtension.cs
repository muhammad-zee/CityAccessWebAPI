using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Web.Data.Models;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;

namespace Web.Services.Helper
{
    public static class HelperExtension
    {

        public static string SplitCamelCase(this string str)
        {
            if (str != null && str != "")
            {
                var splittedString = Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
                return splittedString.First().ToString().ToUpper() + String.Join("", splittedString.Skip(1));
            }
            else
            {
                return "";
            }
        }
        public static string CreateRandomString(int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            //string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }


        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "WebApi1122";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static object RemoveNullValues<T>(this T obj)
        {
            try
            {
               
                    var type = obj.GetType();
                    var returnClass = new ExpandoObject() as IDictionary<string, object>;
                    var props = type.GetProperties();
                    foreach (var propertyInfo in props)
                    {
                        var value = propertyInfo.GetValue(obj);
                        //var valueIsNotAString = value != null;//!(value is string && !string.IsNullOrWhiteSpace(value.ToString()));
                        if (value != null && propertyInfo.Name.ToLower() != "isdeleted" 
                        && propertyInfo.Name.ToLower() != "createddate" && propertyInfo.Name.ToLower() != "createdby"
                        && propertyInfo.Name.ToLower() != "modifieddate" && propertyInfo.Name.ToLower() != "modifiedby"
                         && propertyInfo.Name.ToLower() != "codestrokegroupmembers" && propertyInfo.Name.ToLower() != "iscompleted"
                         && propertyInfo.Name.ToLower() != "capacity" && propertyInfo.Name.ToLower() != "organizationidfk"
                         && (!propertyInfo.Name.ToLower().Contains("code") && !propertyInfo.Name.ToLower().Contains("id")))
                        {
                            returnClass.Add(propertyInfo.Name, value);
                        }
                    }
                    
                return returnClass;
            }
            catch(Exception ex)
            {
                return obj;
            }
        }
   
        public static object getChangedPropertyObject<T>(this T obj,string propertyName)
        {
            try
            {
                var type = obj.GetType();
                var returnClass =new   Dictionary<string, object>();
                var props = type.GetProperties();
                foreach (var propertyInfo in props)
                {
                    var value = propertyInfo.GetValue(obj);
                    //var valueIsNotAString = value != null;//!(value is string && !string.IsNullOrWhiteSpace(value.ToString()));
                    if (value != null && propertyInfo.Name.ToLower() == propertyName.ToLower())
                    {
                        returnClass.Add(propertyInfo.Name, value);
                    }
                }

                return returnClass;
            }
            catch (Exception ex)
            {
                return obj;
            }
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "WebApi1122";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }


        #region Execute Procedure With SQL DataAdaptor

        public static SqlCommand LoadStoredProcedure(
          this DbContext context, string procedureName)
        {
            var cmd = (SqlCommand)context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        public static SqlCommand LoadSQLQuery(
          this DbContext context, string qry)
        {
            var cmd = (SqlCommand)context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = qry;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        public static SqlCommand WithSqlParam(
           this SqlCommand cmd, string paramName, object paramValue)
        {
            if (string.IsNullOrEmpty(cmd.CommandText))
                throw new InvalidOperationException(
                  "Call LoadStoredProc before using this method");
            var param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue;
            cmd.Parameters.Add(param);
            return cmd;
        }

        public static List<T> ExecuteStoredProc<T>(this SqlCommand command)
        {
            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = command;

                    da.Fill(ds);

                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        return ds.MapToList<T>();
                    }
                    else
                    {
                        return new List<T>();
                    }
                    //using (var reader = await command.ExecuteReaderAsync())
                    //{
                    //    return reader.MapToList<T>();
                    //}
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }

        private static List<T> MapToList<T>(this DataSet ds)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();
            var dr = ds.Tables[0];
            var colMapping = dr.Columns.Cast<DataColumn>()
              .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
              .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.Rows.Count > 0)
            {
                foreach (var rows in dr.Rows)
                {
                    var row = (DataRow)rows;
                    T obj = Activator.CreateInstance<T>();
                    foreach (var col in colMapping)
                    {
                        try
                        {
                            string columnName = props.Where(x => x.Name.ToLower() == col.Key.ToLower()).Select(x => x.Name).FirstOrDefault(); //colMapping[prop.Name.ToLower()];
                            //string colName = colMapping[prop.Name.ToLower()].ColumnName;
                            var val = row[columnName];
                            props.Where(x => x.Name.ToLower() == col.Key.ToLower()).FirstOrDefault().SetValue(obj, val == DBNull.Value ? null : val);
                        }
                        catch
                        {
                            props.Where(x => x.Name.ToLower() == col.Key.ToLower()).FirstOrDefault().SetValue(obj, null);
                        }
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static List<Dictionary<string, object>> ExecuteStoredProc_ToDictionary(this SqlCommand command)
        {
            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = command;

                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        var objList = new List<Dictionary<string, object>>();
                        var dr = ds.Tables[0];
                        var colMapping = dr.Columns.Cast<DataColumn>().ToDictionary(key => key.ColumnName);

                        if (dr.Rows.Count > 0)
                        {
                            foreach (var rows in dr.Rows)
                            {
                                var row = (DataRow)rows;
                                var obj = new Dictionary<string, object>();
                                foreach (var col in colMapping)
                                {
                                    try
                                    {   //string colName = colMapping[prop.Name.ToLower()].ColumnName;
                                        var val = row[col.Key.ToLower()];
                                        if (val.ToString() != "")
                                            obj.Add(col.Key.ToCamelCase(), val);
                                        else
                                            obj.Add(col.Key.ToCamelCase(), null);
                                    }
                                    catch
                                    {
                                        obj.Add(col.Key, null);
                                    }
                                }
                                objList.Add(obj);
                            }
                        }
                        return objList;
                    }
                    else
                    {
                        return new List<Dictionary<string, object>>();
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }

        public static int ExecuteInsertQuery(
          this DbContext context, string qry)
        {
            var cmd = (SqlCommand)context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = qry+ "  SELECT SCOPE_IDENTITY()";
            cmd.CommandType = CommandType.Text;
            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            int Id = Convert.ToInt32(cmd.ExecuteScalar());

            if (cmd.Connection.State == ConnectionState.Open)
                cmd.Connection.Close();

            return Id;
        }

        #endregion

        public static string ToCamelCase(this string str)
        {
            var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                m =>
                {
                    return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
                });
            var tailWords = words.Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();
            return $"{leadWord}{string.Join(string.Empty, tailWords)}";
        }

       

    }
}

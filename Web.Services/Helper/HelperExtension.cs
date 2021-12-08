﻿using ElmahCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web.Model.Common;

namespace Web.API.Helper
{
    public static class HelperExtension
    {
        public static string CreateRandomPassword(string firstName, int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            var strongPswd = new string(chars);
            var firstHalf = strongPswd.Substring(0, (int)(strongPswd.Length / 2));
            var lastHalf = strongPswd.Substring((int)(strongPswd.Length / 2), (int)(strongPswd.Length / 2));
            return firstHalf + firstName + lastHalf;
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "RAQWebApi1122";
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

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "RAQWebApi1122";
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

        public static IList<TreeviewItemVM> BuildTree(this IEnumerable<TreeviewItemVM> source)
        {
            var groups = source.GroupBy(i => i.ParentKey);

            var roots = groups.FirstOrDefault(g => g.Key.HasValue == false).ToList();

            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value.ToString(), g => g.ToList());
                for (int i = 0; i < roots.Count; i++)
                    AddChildren(roots[i], dict);
            }

            return roots;
        }

        private static void AddChildren(TreeviewItemVM node, IDictionary<string, List<TreeviewItemVM>> source)
        {
            if (source.ContainsKey(node.key))
            {
                node.children = source[node.key];
                for (int i = 0; i < node.children.Count; i++)
                    AddChildren(node.children[i], source);
            }
            else
            {
                node.children = new List<TreeviewItemVM>();
                node.expanded = false;
            }
        }


        public static IList<ComponentAccessByRoleAndUserTreeVM> BuildComponentAccessTree(this IEnumerable<ComponentAccessByRoleAndUserTreeVM> source)
        {
            var groups = source.GroupBy(i => i.ParentComponentId);

            //var roots = groups.FirstOrDefault(g => g.Key.HasValue == false).ToList();
           var roots = source.Where(s => !s.IsAction ).ToList();
            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value.ToString(), g => g.ToList());
                for (int i = 0; i < roots.Count; i++)
                    AddActionList(roots[i], dict);
            }
            return roots;
        }

        private static void AddActionList(ComponentAccessByRoleAndUserTreeVM node, IDictionary<string, List<ComponentAccessByRoleAndUserTreeVM>> source)
        {
            if (source.ContainsKey(node.ComponentId))
            {
                node.children = source[node.ComponentId];
                //for (int i = 0; i < node.children.Count; i++)
                //    AddActionList(node.children[i], source);
                if (node.Actions == null)
                {
                    node.Actions = new List<string>();
                }
                foreach (var child in node.children)
                {

                    node.Actions.Add(child.ModuleName);
                }
            }
            else
            {
                node.children = new List<ComponentAccessByRoleAndUserTreeVM>();
            }
        }


        public static DbCommand LoadStoredProc(
           this DbContext context, string storedProcName)
        {
            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = storedProcName;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return cmd;
        }

        public static DbCommand WithSqlParam(
            this DbCommand cmd, string paramName, object paramValue)
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

        private static List<T> MapToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
              .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
              .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        try
                        {
                            var columnName = colMapping[prop.Name.ToLower()];
                            var val =
                              dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                            prop.SetValue(obj, val == DBNull.Value ? null : val);
                        }
                        catch 
                        {
                            prop.SetValue(obj, null);
                        }
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static async Task<List<T>> ExecuteStoredProc<T>(this DbCommand command)
        {
            using (command)
            {
                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return reader.MapToList<T>();
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

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Web.Services.Extensions
{
    public static class CSVExtensions
    {
        /// <summary>
        /// Serialize objects to Comma Separated Value (CSV) format [1].
        /// 
        /// Rather than try to serialize arbitrarily complex types with this
        /// function, it is better, given type A, to specify a new type, A'.
        /// Have the constructor of A' accept an object of type A, then assign
        /// the relevant values to appropriately named fields or properties on
        /// the A' object.
        /// </summary>
        public static string ToCSV<T>(this IEnumerable<T> objects, Type type = null, string CsvSeparator = ",")
        {
            StringBuilder output = new StringBuilder();
            var fields =
                from mi in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                where new[] { MemberTypes.Field, MemberTypes.Property }.Contains(mi.MemberType)

                select mi;

            if (type != null)
            {

                PropertyInfo[] props = type.GetProperties();

                List<string> record = new List<string>();

                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        DisplayNameAttribute authAttr = attr as DisplayNameAttribute;
                        if (authAttr != null)
                        {
                            string fieldName = authAttr.DisplayName;
                            record.Add(fieldName);
                        }
                    }
                }

                output.AppendLine(QuoteRecord(record, CsvSeparator));
            }

            else
            {
                output.AppendLine(QuoteRecord(fields.Select(f => f.Name.Replace("_", " ").Replace("dot", ".")), CsvSeparator));
            }

            foreach (var record in objects)
            {
                output.AppendLine(QuoteRecord(FormatObject(fields, record), CsvSeparator));
            }
            return output.ToString();
        }

        public static string ToHeadlessCSV<T>(this IEnumerable<T> objects, Type type = null, string CsvSeparator = ",")
        {
            StringBuilder output = new StringBuilder();
            var fields =
                from mi in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                where new[] { MemberTypes.Field, MemberTypes.Property }.Contains(mi.MemberType)

                select mi;

            foreach (var record in objects)
            {
                output.AppendLine(QuoteRecord(FormatObject(fields, record), CsvSeparator));
            }
            return output.ToString();
        }

        static IEnumerable<string> FormatObject<T>(IEnumerable<MemberInfo> fields, T record)
        {
            foreach (var field in fields)
            {
                if (field is FieldInfo)
                {
                    var fi = (FieldInfo)field;
                    yield return Convert.ToString(fi.GetValue(record));
                }
                else if (field is PropertyInfo)
                {
                    var pi = (PropertyInfo)field;
                    yield return Convert.ToString(pi.GetValue(record, null));
                }
                else
                {
                    throw new Exception("Unhandled case.");
                }
            }
        }

        //const string CsvSeparator = ",";

        static string QuoteRecord(IEnumerable<string> record, string CsvSeparator)
        {
            return String.Join(CsvSeparator, record.Select(field => QuoteField(field, CsvSeparator)).ToArray());
        }

        static string QuoteField(string field, string CsvSeparator)
        {
            if (String.IsNullOrEmpty(field))
            {
                return "\"\"";
            }
            else if (field.Contains(CsvSeparator) || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return String.Format("\"{0}\"", field.Replace("\"", "\"\""));
            }
            else
            {
                return field;
            }
        }

        //CSV generated using Datatable
        public static string ToCSV(this DataTable table)
        {
            var result = new StringBuilder();

            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            // write rest of the data
            foreach (DataRow row in table.Rows)
            {
                //result = new StringBuilder();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string strData = Convert.ToString(row[i]);
                    strData = strData.Replace("\"", "|~");// remove double quotes with |~
                    strData = strData.Replace("'", "|!"); // replace single quotes with |!
                    strData = strData.Replace(",", "||"); // replace comma with ||
                    strData = strData.Replace("\\", "|^"); // replace slash with |^
                    strData = strData.Replace("\r", ""); // remove \r
                    strData = strData.Replace("\n", ""); // remove \n

                    result.Append(strData);
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return result.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnOrderAttribute : Attribute
    {
        public int Order { get; private set; }
        public ColumnOrderAttribute(int order) { Order = order; }
    }

    public static class FilesExtensions
    {
        public static string GetFileExtenstion(this string extenstion)
        {
            if (extenstion.Contains("image/jpg"))
                return ".jpg";
            if (extenstion.Contains("image/jpeg"))
                return ".jpeg";
            if (extenstion.Contains("image/png"))
                return ".png";
            if (extenstion.Contains("audio/mpeg"))
                return ".mp3";
            if (extenstion.Contains("video/mp4"))
                return ".mp4";
            if (extenstion.Contains("video/x-matroska"))
                return ".mkv";
            if (extenstion.Contains("application/octet-stream"))
                return ".flv";
            if (extenstion.Contains("application/pdf"))
                return ".pdf";
            if (extenstion.Contains("application/msword"))
                return ".doc";
            if (extenstion.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
                return ".docx";
            if (extenstion.Contains("application/vnd.ms-excel"))
                return ".xls";
            else
                return null;
        }
    }
}


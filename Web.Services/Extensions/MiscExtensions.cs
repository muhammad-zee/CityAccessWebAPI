using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services.Extensions
{
    public static class MiscExtensions
    {
        //public static Dictionary<string, string> GetModalErrors(this System.Web.Mvc.Controller controller)
        //{

        //    var dict = new Dictionary<string, string>();

        //    foreach (var item in controller.ModelState)
        //    {
        //        string key = item.Key.ToLower().Trim();

        //        foreach (var subitem in item.Value.Errors)
        //        {

        //            var isexists = dict.ContainsKey(key);
        //            if (isexists)
        //            {
        //                dict[key] = dict[key] + "  " + subitem.ErrorMessage;
        //            }
        //            else
        //            {
        //                dict.Add(key, subitem.ErrorMessage);
        //            }
        //        }
        //    }
        //    return dict;

        //}

        public static void Add(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }     


        /// <summary>
        /// Return age of a person in years
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="birth_date"></param>
        /// <returns></returns>
        public static int CalculateAge(this DateTime dt, DateTime birth_date)
        {
            int age = dt.Year - birth_date.Year;

            if (dt.Month < birth_date.Month || (dt.Month == birth_date.Month && dt.Day < birth_date.Day))
                age--;

            return age;

            //double days = (dt - birth_date).TotalDays;
            //var years = Math.Floor(days / 365.0);
            //return years.ToInt();
        }

        public static StringBuilder AppendItem(this StringBuilder builder, string formattedText, string fieldText)
        {
            if (!string.IsNullOrEmpty(fieldText))
            {
                builder = builder.Append(formattedText);
            }
            return builder;
        }

        public static StringBuilder AppendSqlParam(this StringBuilder builder, string key, object value)
        {
            builder.Append(key + "='" + value + "'");
            return builder;
        }

        public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
    }
}

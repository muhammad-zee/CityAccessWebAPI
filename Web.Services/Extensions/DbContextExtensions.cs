using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Web.Data.Models;

namespace Web.Services.Extensions
{
    public static class DbContextExtensions
    {
        //public static List<Dictionary<string, object>> SqlToList(this RAQ_DbContext context, string sqlQuery, DbTransaction transaction, string includeColumns = "")
        //{
        //    using (var cmd = context.Database.Connection.CreateCommand())
        //    {
        //        context.Database.CommandTimeout = 1800;

        //        if (context.Database.Connection.State != System.Data.ConnectionState.Open)
        //            context.Database.Connection.Open();

        //        if (transaction != null) cmd.Transaction = transaction;

        //        cmd.CommandText = sqlQuery;

        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            return reader.ReaderToList(includeColumns);
        //        }
        //    }
        //}


        //public static IQueryable<T> WhereEquals<T>(this IQueryable<T> source, string member, object value, PropertyInfo property = null)
        //{

        //    if (property == null)
        //        property = source.GetType().GetGenericArguments()[0].GetProperty(member);

        //    Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        //    object safeValue = null;
        //    if (property.PropertyType.Name == "Guid")
        //    {
        //        Guid temp;

        //        if (Guid.TryParse(Convert.ToString(value), out temp))
        //            safeValue = Convert.ChangeType(temp, t);
        //    }
        //    else
        //        safeValue = (value == null) ? null : Convert.ChangeType(value, t);


        //    if (safeValue.GetType() == typeof(DateTime))
        //    {

        //        var dt = Convert.ToDateTime(safeValue);
        //        var startDate = new DateTime(dt.Year, dt.Month, dt.Day, 00, 00, 00, 00);
        //        var endDate = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 59);
        //        source = source.Where(member + " >= @0 && " + member + " <= @1", startDate, endDate);
        //        return source;
        //    }
        //    else
        //    {
        //        return source.Where(member + " = @0", safeValue);
        //    }

        //}

        /*
        public static IQueryable<T> WhereCriteria<T>(this IQueryable<T> source, string member, object value, string comparisonOperator, PropertyInfo property = null)
        {
            if (property == null)
                property = source.GetType().GetGenericArguments()[0].GetProperty(member);

            Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            object safeValue = null;
            safeValue = (value == null) ? null : Convert.ChangeType(value, t);

            if (safeValue.GetType() == typeof(Int32))
            {
                switch (comparisonOperator)
                {
                    case "Equal":
                        source = source.Where(member + " = @0", safeValue);
                        break;
                    case "GreaterThan":
                        source = source.Where(member + " > @0", safeValue);
                        break;
                    case "GreaterThanOrEqual":
                        source = source.Where(member + " >= @0", safeValue);
                        break;
                    case "LessThan":
                        source = source.Where(member + " < @0", safeValue);
                        break;
                    case "LessThanOrEqual":
                        source = source.Where(member + " <= @0", safeValue);
                        break;
                    case "NotEqual":
                        source = source.Where(member + " != @0", safeValue);
                        break;
                    default:
                        break;
                }
            }
            return source;
        }
        */

        //public static IQueryable<T> WhereCriteria<T>(this IQueryable<T> source, string member, int value, string comparisonOperator)
        //{
        //    comparisonOperator = comparisonOperator.Replace("eq", "Equal")
        //                                           .Replace("gt", "GreaterThan")
        //                                           .Replace("lt", "LessThan")
        //                                           .Replace("neq", "NotEqual");


        //    switch (comparisonOperator)
        //    {
        //        case "Equal":
        //            source = source.Where(member + " = @0", value);
        //            break;
        //        case "GreaterThan":
        //            source = source.Where(member + " > @0", value);
        //            break;
        //        case "GreaterThanOrEqual":
        //            source = source.Where(member + " >= @0", value);
        //            break;
        //        case "LessThan":
        //            source = source.Where(member + " < @0", value);
        //            break;
        //        case "LessThanOrEqual":
        //            source = source.Where(member + " <= @0", value);
        //            break;
        //        case "NotEqual":
        //            source = source.Where(member + " != @0", value);
        //            break;
        //        default:
        //            break;
        //    }
        //    return source;
        //}

        //public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, Kendo.DynamicLinq.DataSourceRequest gridFilters)
        //{

        //    foreach (var filter in gridFilters.Filter.Filters)
        //    {
        //        if (filter.Value != null)
        //        {
        //            if (!string.IsNullOrEmpty(filter.Value?.ToString()))
        //            {
        //                source = source.WhereEquals(filter.Field, filter.Value);
        //            }
        //        }
        //    }
        //    return source;
        //}

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static List<Dictionary<string, object>> ReaderToList(this DbDataReader reader, string includeColumns = "")
        {
            var expandolist = new List<Dictionary<string, object>>();
            var listColumns = new List<string>();

            // convert columns to list base on comma
            if (!string.IsNullOrEmpty(includeColumns))
                listColumns = includeColumns.Split(',').ToList();

            while (reader.Read())
            {
                IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);

                    if (listColumns.Count() == 0 || listColumns.Contains(columnName))
                    {
                        expando.Add(Convert.ToString(columnName), reader.GetValue(i));
                    }
                }

                expandolist.Add(new Dictionary<string, object>(expando));
            }

            return expandolist;
        }
    }
}

using Dapper;
using AngularJSAuthRefreshToken.Data.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.MSSQL.Extensions
{
    public static class BrowserContextExtensions
    {
        public static readonly string sqlPageingWrapper = @"{0}
 OFFSET @Offset ROWS
 FETCH NEXT @PageSize ROWS ONLY";

        public class Location
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int Offset { get; set; }
        }

        public static int PageValue(this int? page)
        {
            int result = (page ?? 1);
            if (result < 1)
                result = 1;

            return result;
        }

        public static int PageSizeValue(this int? pageSize)
        {
            int result = (pageSize ?? 10);
            if (result <= 0)
                result = 10;

            return result;
        }

        public static Location GetLocation(this BrowserContext context)
        {
            var p = context.Page.PageValue();
            var ps = context.PageSize.PageSizeValue();
            var offset = (p - 1) * ps;
            return new Location { Page = p, PageSize = ps, Offset = offset };
        }

        public static string AddWhereSQL(string sql, string searchByCondition, bool where = false)
        {
            if (!string.IsNullOrWhiteSpace(searchByCondition))
            {
                if (where)
                {
                    sql += " AND (" + searchByCondition + ")";
                }
                else
                {
                    sql += @"
WHERE " + searchByCondition;
                }
            }
            return sql;
        }

        public static string AddOrderBySQL(string sql, string orderByCondition, bool orderBy = false)
        {
            if (!string.IsNullOrWhiteSpace(orderByCondition))
            {
                if (orderBy)
                {
                    sql += ", " + orderByCondition;
                }
                else
                {
                    sql += @"
ORDER BY " + orderByCondition;
                }
            }
            return sql;
        }

        public static string GetPageingSQL(string sql)
        {
            return string.Format(sqlPageingWrapper, sql);
        }

        public static PagedResult<T> CreatePagedResult<T>(this IEnumerable<T> context, BrowserContext browserContext)
            where T : class
        {
            var coll = context as ICollection<T>;
            var location = browserContext.GetLocation();
            var lastPage = browserContext.LastPage ?? 1;
            if (lastPage < location.Page)
            {
                lastPage = location.Page;
            }

            if (coll.Count < browserContext.PageSize)
            {
                if ((lastPage * browserContext.PageSize) > (location.Offset + coll.Count))
                {
                    lastPage = browserContext.Page.Value;
                }
            }

            return new PagedResult<T>
            {
                Collection = coll,
                Count = (lastPage - 1) * location.PageSize + coll.Count,
                Sort = browserContext.Sort,
                SortDir = browserContext.Sortdir.Value
            };
        }

        public static void GetParameters(this BrowserContext browserContext,
            ref DynamicParameters param, ref string searchByCondition, ref string orderByCondition,
            Dictionary<string, Column> columns, string defaultSortColumn, object contextId = null)
        {
            Column column; object[] parsedValues=null; string pattern;
            if (null == browserContext)
            {
                throw new BrowserContextException("BrowserContext can't be null.");
            }

            if (string.IsNullOrWhiteSpace(browserContext.Sort))
            {
                browserContext.Sort = defaultSortColumn;
            }

            if (!browserContext.Sortdir.HasValue)
            {
                browserContext.Sortdir = SortDir.asc;
            }

            if (columns.TryGetValue(browserContext.Sort, out column) && column.Sortable)
            {
                orderByCondition = column.Name;
                column = null;
            }
            else
            {
                throw new BrowserContextException("Invalid sort column {0}.", browserContext.Sort);
            }

            if (!string.IsNullOrWhiteSpace(browserContext.SearchColumn))
            {
                if (columns.TryGetValue(browserContext.SearchColumn, out column) && column.Searchable)
                {
                    searchByCondition = column.Name;
                }
                else
                {
                    throw new BrowserContextException("Invalid search column {0}.", browserContext.SearchColumn);
                }
            }

            if (null != column && browserContext.SearchValue.IsEmptyValues(column, out parsedValues))
            {
                throw new BrowserContextException("Search column can't be empty when value is present.");
            }

            var location = browserContext.GetLocation();
            if (null == param)
            {
                param = new DynamicParameters();
            }

            if (!IsEmpty(contextId))
            {
                param.Add("ContextId", value: contextId);
            }

            param.Add("PageSize", value: location.PageSize);
            param.Add("Offset", value: location.Offset);

            if (column == null)
            {
                searchByCondition = null;
            }
            else
            {
                switch (column.DataType)
                {
                    case DataType.Integer:
                    case DataType.Number:
                    case DataType.Date:
                        if (parsedValues.Length > 1)
                        {
                            param.Add("SearchValue0", value: parsedValues[0]);
                            param.Add("SearchValue1", value: parsedValues[1]);
                            pattern = "({0} between @SearchValue0 AND @SearchValue1)";
                        }
                        else
                        {
                            param.Add("SearchValue", value: parsedValues[0]);
                            pattern = "({0} = @SearchValue)";
                        }
                        break;
                    case DataType.Boolean:
                        param.Add("SearchValue", value: parsedValues[0]);
                        pattern = "({0} = @SearchValue)";
                        break;
                    default:
                        param.Add("SearchValue", value: ((string)parsedValues[0]).ApplySearchSymbols());
                        pattern = "({0} LIKE @SearchValue)";
                        break;
                }
                searchByCondition = string.Format(pattern, column.Name);
            }
            orderByCondition = string.Format(orderByCondition + " {0}", browserContext.Sortdir);
        }


        public static string ApplySearchSymbols(this string searchValue)
        {
            if (!searchValue.StartsWith("%"))
            {
                searchValue = "%" + searchValue;
            }
            if (!searchValue.EndsWith("%"))
            {
                searchValue += "%";
            }
            return searchValue;
        }

        public static bool IsEmpty(object value)
        {
            if (value is string)
            {
                return string.IsNullOrWhiteSpace(value as string);
            }
            else
            {
                return value == null; 
            }
        }

        public static bool IsEmpty(this string[] values)
        {
            if (null == values || 0 == values.Length)
            {
                return true;
            }

            if (values.Where(item => !string.IsNullOrWhiteSpace(item)).Count() == 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsEmptyValues(this string[] values, Column column, out object[] parsedValues)
        {
            parsedValues = new object[0];
            if (values.IsEmpty())
            {
                return true;
            }

            switch (column.DataType)
            {
                case DataType.Boolean:
                    parsedValues = values
                        .Select((item) =>
                        {
                            bool val;
                            if (bool.TryParse(item, out val))
                            {
                                return (object)val;
                            }
                            else
                            {
                                double num;
                                if (double.TryParse(item, out num))
                                {
                                    return (object)(num != 0);
                                }
                                else
                                {
                                    return (object)null;
                                }
                            }
                        })
                        .Where(item => null != item)
                        .ToArray();
                    break;
                case DataType.Integer:
                    parsedValues = values
                        .Select((item) =>
                        {
                            long val;
                            if (long.TryParse(item, out val))
                            {
                                return (object)val;
                            }
                            else
                            {
                                return (object)null;
                            }
                        })
                        .Where(item => null != item)
                        .ToArray();
                    break;
                case DataType.Number:
                    parsedValues = values
                        .Select((item) =>
                        {
                            decimal val;
                            if (decimal.TryParse(item, out val))
                            {
                                return (object)val;
                            }
                            else
                            {
                                return (object)null;
                            }
                        })
                        .Where(item => null != item)
                        .ToArray();
                    break;
                case DataType.Date:
                    parsedValues = values
                        .Select((item) =>
                        {
                            DateTime val;
                            if (DateTime.TryParseExact(item, new[] { "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd" }, null, System.Globalization.DateTimeStyles.None, out val))
                            {
                                return (object)val;
                            }
                            else
                            {
                                return (object)null;
                            }
                        })
                        .Where(item => null != item)
                        .ToArray();
                    break;
                default:
                    parsedValues = values
                        .Select((item) =>
                        {
                            switch (column.Case)
                            {
                                case ValueCase.Lower:
                                    return (object)item.ToLower();
                                case ValueCase.Upper:
                                    return (object)item.ToUpper();
                                default:
                                    return (object)item;
                            }
                        })
                        .ToArray();
                    break;
            }
            return values.Length != parsedValues.Length;
        }
    }
}

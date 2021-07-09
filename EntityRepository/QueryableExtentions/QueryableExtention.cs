using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityRepository.QueryableExtentions
{
    internal static class QueryableExtention
    {
        /// <summary>
        /// DbContext Extention - Method gets and returns the first property which matches the Generic Type DbSet<T> in the extended DbContext
        /// </summary>
        /// <typeparam name="EntityType">class of specified DbSet</typeparam>
        /// <param name="dbContext">Extended DbSet</param>
        /// <returns>First found DbSet</returns>
        public static DbSet<EntityType> RouteByEntity<EntityType>(this DbContext dbContext) where EntityType : class
        {
            if (dbContext == null) throw new ArgumentNullException($"The given context is null. DbSet<{nameof(EntityType)}> not found");

            var contextType = dbContext.GetType();
            var props = contextType.GetProperties();

            if (!props.Any()) throw new ArgumentException($"The given context does not define DbSets. DbSet<{nameof(EntityType)}> not found");

            var matchProps = props.Where(x => x.PropertyType.Equals(typeof(DbSet<EntityType>))).FirstOrDefault();

            if (matchProps != null)
            {
                return matchProps.GetValue(dbContext) as DbSet<EntityType>;
            }
            else
            {
                throw new ArgumentException($"The given entity route does not exists. DbSet<{nameof(EntityType)}> not found");
            }

        }

        /// <summary>
        /// Appends the Query with Optional Tracking Statement
        /// </summary>
        /// <typeparam name="T">Gerneric Type of UsageBase</typeparam>
        /// <param name="query">Ef6 Queryable</param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public static IQueryable<T> AppendTracking<T>(this IQueryable<T> query, bool enabled) where T : class
        {
            if (enabled)
            {
                return query;
            }
            else
            {
                return query.AsNoTracking();
            }
        }

        /// <summary>
        /// Appends the Take Statement to the iQueryable defaults to Null which means all
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="rowlimit"></param>
        /// <returns></returns>
        public static IQueryable<T> AppendMaxRowCount<T>(this IQueryable<T> query, int? rowlimit = null) where T : class
        {
            if (!rowlimit.HasValue)
            {
                return query;
            }
            else
            {
                return query.Take(rowlimit.Value);
            }

        }

        /// <summary>
        /// Appends whercondition to the Iqueryable statement 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<T> AppendCondition<T>(this IQueryable<T> query, Func<T, bool> condition) where T : class
        {
            if (condition == null)
            {
                return query;
            }
            else
            {
                return query.Where(condition);
            }
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
            {
                throw new Exception("Source or/and Destination Objects are null");
            }
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and  
            // populate them from their desination counterparts  
            PropertyInfo[] srcProps = typeSrc.GetProperties();

            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }
                // Passed all tests, lets set the value
                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }



    }
}

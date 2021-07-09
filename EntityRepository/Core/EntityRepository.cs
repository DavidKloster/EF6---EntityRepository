using logm.EntityRepository.QueryableExtentions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logm.EntityRepository.Core
{
    /// <summary>
    /// Generic Abstract Repository Class for easy and Uncomplicated Dataccess 
    /// </summary>
    /// <typeparam name="DatabaseContext">Gerneric Typeparameter of the Database context</typeparam>
    /// <typeparam name="T">Generic Objecttype used by DataSet Router</typeparam>
    public abstract class EntityRepository<DatabaseContext, T> where DatabaseContext : DbContext, IDisposable where T : class
    {
        private static DatabaseContext CreateDatabaseContextInstance()
        {
            return (DatabaseContext)Activator.CreateInstance(typeof(DatabaseContext), new object[] { });
        }


        public static async Task<List<T>> GetAllAsync(Func<T, bool> condition = null, int? maxResultSize = null, bool trackingEnabled = true)
        {
            return await Task.Run(() =>
            {
                using (var context = CreateDatabaseContextInstance())
                {
                    return context.RouteByEntity<T>().AppendMaxRowCount(maxResultSize).AppendTracking(trackingEnabled).AppendCondition(condition).ToList();
                }
            });
        }

        public static List<T> GetAll(Func<T, bool> condition = null, int? maxResultSize = null)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                if (condition == null)
                {
                    return context.RouteByEntity<T>().AppendMaxRowCount(maxResultSize).AsNoTracking().ToList();
                }
                else
                {
                    return context.RouteByEntity<T>().AppendMaxRowCount(maxResultSize).AsNoTracking().Where(condition).ToList();
                }
            }
        }

        public static async Task<T> GetSingleAsync(Func<T, bool> condition = null)
        {
            return await Task.Run(() =>
            {

                using (var context = CreateDatabaseContextInstance())
                {
                    if (condition == null)
                    {
                        return context.RouteByEntity<T>().FirstOrDefault();
                    }
                    else
                    {
                        return context.RouteByEntity<T>().Where(condition).FirstOrDefault();
                    }
                }
            });
        }

        public static T GetSingle(Func<T, bool> condition = null)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                if (condition == null)
                {
                    return context.RouteByEntity<T>().FirstOrDefault();
                }
                else
                {
                    return context.RouteByEntity<T>().Where(condition).FirstOrDefault();
                }
            }
        }

        public static async Task<T> CreateAsync(T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                context.RouteByEntity<T>().Add(obj);
                await context.SaveChangesAsync();
                return obj;
            }
        }

        public static T Create(T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                context.RouteByEntity<T>().Add(obj);
                context.SaveChanges();
                return obj;
            }
        }

        public static async Task<T> ModifyAsync(Func<T, bool> condition, T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                var objs = context.RouteByEntity<T>().Where(condition).ToList();
                if (objs.Count > 1) throw new NotSupportedException($"Condition {nameof(condition)} does not return Single Value");
                if (!objs.Any()) return null;

                T objtoedit = objs.First();

                obj.CopyProperties(objtoedit);

                await context.SaveChangesAsync();
                return objtoedit;
            }
        }

        public static T Modify(Func<T, bool> condition, T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                var objs = context.RouteByEntity<T>().Where(condition).ToList();
                if (objs.Count > 1) throw new NotSupportedException($"Condition {nameof(condition)} does not return Single Value");
                if (!objs.Any()) return null;

                T objtoedit = objs.First();

                obj.CopyProperties(objtoedit);

                context.SaveChanges();
                return objtoedit;
            }
        }

        public static async Task<bool> DeleteAsync(Func<T, bool> condition)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                List<T> objstodelete = context.RouteByEntity<T>().Where(condition).ToList();

                foreach (var ob in objstodelete)
                {
                    context.RouteByEntity<T>().Remove(ob);
                }

                await context.SaveChangesAsync();

                return true;
            }
        }



        public static async Task<bool> DeleteAsync(T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {

                context.RouteByEntity<T>().Remove(obj);

                await context.SaveChangesAsync();

                return true;
            }
        }

        public static bool Delete(Func<T, bool> condition)
        {
            using (var context = CreateDatabaseContextInstance())
            {
                List<T> objstodelete = context.RouteByEntity<T>().Where(condition).ToList();

                foreach (var ob in objstodelete)
                {
                    context.RouteByEntity<T>().Remove(ob);
                }

                context.SaveChangesAsync();

                return true;
            }
        }

        public static bool Delete(T obj)
        {
            using (var context = CreateDatabaseContextInstance())
            {

                context.RouteByEntity<T>().Remove(obj);

                context.SaveChangesAsync();

                return true;
            }
        }


    }

}

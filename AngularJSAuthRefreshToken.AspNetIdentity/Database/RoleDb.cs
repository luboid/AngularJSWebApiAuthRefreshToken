using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using LL.Repository;
using System.Data.SqlClient;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    internal static class RoleDb
    {
        static async Task<int> UpdateAsync(this IDbConnectionContext ctx, IdentityRole role)
        {
            return await ctx.Connection.ExecuteAsync(@"UPDATE AspNetRoles SET Name = @Name WHERE Id = @Id",
                    param: role,
                    transaction: ctx.Transaction);
        }

        static async Task<int> InsertAsync(this IDbConnectionContext ctx, IdentityRole role)
        {   
            return await ctx.Connection.ExecuteAsync(@"INSERT INTO AspNetRoles(Id,Name) VALUES(@Id,@Name)",
                        param: role,
                        transaction: ctx.Transaction);
        }

        public static async Task InsertOrUpdateAsync(this IDbContext context, IdentityRole role)
        {
            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Guid.NewGuid().ToString("D");
            }
            else
            {
                role.Id.ToLower();
            }

            using (var ctx = context.BeginTransaction())
            {
                int update = await ctx.UpdateAsync(role);

                if (0 == update)
                {
                    await ctx.InsertAsync(role);
                }

                ctx.Commit();
            }
        }

        public static async Task DeleteAsync(this IDbContext context, IdentityRole role)
        {
            using (var ctx = context.BeginTransaction())
            {
                await ctx.Connection.ExecuteAsync(@"DELETE FROM AspNetRoles WHERE Id = @Id",
                    param: new { role.Id },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static IQueryable<IdentityRole> GetRoles(this IDbContext context)
        {
            using (var ctx = context.Open())
                return ctx.Connection.Query<IdentityRole>(
                    sql: "SELECT * FROM AspNetRoles ORDER BY Name", 
                    transaction: ctx.Transaction).AsQueryable();
        }

        public static async Task<IdentityRole> FindRoleByIdAsync(this IDbContext context, string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityRole>("SELECT * FROM AspNetRoles WHERE Id = @id",
                        param: new { id },
                        transaction: ctx.Transaction)).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static async Task<IdentityRole> FindRoleByNameAsync(this IDbContext context, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityRole>("SELECT * FROM AspNetRoles WHERE Name = @name",
                        param: new { name },
                        transaction: ctx.Transaction)).FirstOrDefault();
            }
        }
    }
}

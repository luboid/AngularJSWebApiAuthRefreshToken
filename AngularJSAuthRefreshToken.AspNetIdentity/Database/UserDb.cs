using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;

using AngularJSAuthRefreshToken.AspNetIdentity.Properties;
using AngularJSAuthRefreshToken.Data.Browser;

using LL.Repository;
using LL.MSSQL.Extensions;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    internal static class UserDb
    {
        static readonly Dictionary<string, Column> userBrowserContextColumns = new Dictionary<string, Column>(StringComparer.CurrentCultureIgnoreCase) {
            {"userName", new Column{Name= "UserName"}},
            {"email", new Column{Name= "Email"}},
            {"emailConfirmed", new Column{Name= "emailConfirmed", DataType=DataType.Boolean, Sortable=false}},
            {"phoneNumber", new Column{Name= "PhoneNumber",Sortable=false}},
            {"phoneNumberConfirmed", new Column{Name= "PhoneNumberConfirmed", DataType=DataType.Boolean, Sortable=false, Searchable=false}}
        };

        public static async Task<PagedResult<DTO.IdentityUser>> BrowseUsers(this IDbContext context, BrowserContext browserContext)
        {
            DynamicParameters param = null; 
            string searchByCondition = null, 
                orderByCondition = null, 
                sql = null;

            browserContext.GetParameters(ref param, ref searchByCondition, ref orderByCondition, 
                userBrowserContextColumns, "email");

            sql = "SELECT Id,UserName,Email,EmailConfirmed,PhoneNumber,PhoneNumberConfirmed FROM AspNetUsers";

            sql = BrowserContextExtensions.AddWhereSQL(sql, searchByCondition);
            sql = BrowserContextExtensions.AddOrderBySQL(sql, orderByCondition);
            sql = BrowserContextExtensions.GetPageingSQL(sql);

            using (var ctx = context.Open())
                return (await ctx.Connection.QueryAsync<DTO.IdentityUser>(sql: sql, param: param, transaction: ctx.Transaction))
                    .CreatePagedResult(browserContext);
        }

        public static async Task InsertOrUpdateAsync(this IDbContext context, IdentityUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString("D");
            }

            user.Email = (user.Email ?? "").ToLower();
            user.UserName = (user.UserName ?? "").ToLower();

            using (var ctx = context.BeginTransaction())
            {

                int update = (await ctx.Connection.ExecuteAsync(sql: @"UPDATE AspNetUsers
   SET Email = @Email
      ,EmailConfirmed = @EmailConfirmed
      ,PasswordHash = @PasswordHash
      ,SecurityStamp = @SecurityStamp
      ,PhoneNumber = @PhoneNumber
      ,PhoneNumberConfirmed = @PhoneNumberConfirmed
      ,TwoFactorEnabled = @TwoFactorEnabled
      ,LockoutEndDateUtc = @LockoutEndDateUtc
      ,LockoutEnabled = @LockoutEnabled
      ,AccessFailedCount = @AccessFailedCount
      ,UserName = @UserName
      ,PushRegistrationId = @PushRegistrationId
      ,Confirmed = @Confirmed
 WHERE Id = @Id", param: user, transaction: ctx.Transaction));

                if (0 == update)
                {
                    await ctx.Connection.ExecuteAsync(sql: @"INSERT INTO AspNetUsers
           (Id
           ,Email
           ,EmailConfirmed
           ,PasswordHash
           ,SecurityStamp
           ,PhoneNumber
           ,PhoneNumberConfirmed
           ,TwoFactorEnabled
           ,LockoutEndDateUtc
           ,LockoutEnabled
           ,AccessFailedCount
           ,UserName
           ,PushRegistrationId
           ,Confirmed)
     VALUES
           (@Id
           ,@Email
           ,@EmailConfirmed
           ,@PasswordHash
           ,@SecurityStamp
           ,@PhoneNumber
           ,@PhoneNumberConfirmed
           ,@TwoFactorEnabled
           ,@LockoutEndDateUtc
           ,@LockoutEnabled
           ,@AccessFailedCount
           ,@UserName
           ,@PushRegistrationId
           ,@Confirmed)", param: user, transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static async Task DeleteAsync(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.BeginTransaction())
            {
                await ctx.Connection.ExecuteAsync(
                    sql: @"DELETE FROM AspNetUsers WHERE Id = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static async Task<IList<Claim>> GetClaimsAsync(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return (await ctx.Connection.QueryAsync(
                        sql: @"SELECT * FROM AspNetUserClaims WHERE UserId = @Id ORDER BY ClaimType, ClaimValue",
                        param: new { user.Id },
                        transaction: ctx.Transaction))
                        .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                        .ToList<Claim>();
        }

        public static async Task AddClaimAsync(this IDbContext context, IdentityUser user, Claim claim)
        {
            using (var ctx = context.BeginTransaction())
            {
                var id = (await ctx.Connection.QueryAsync<string>(
                        sql: @"SELECT Id FROM AspNetUserClaims WHERE UserId = @UserId AND ClaimType = @ClaimType AND ClaimValue = @ClaimValue",
                        param: new {
                            UserId = user.Id,
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value
                        },
                        transaction: ctx.Transaction)).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    await ctx.Connection.ExecuteAsync(
                        sql: @"INSERT INTO AspNetUserClaims(Id,UserId,ClaimType,ClaimValue) VALUES(@Id,@UserId,@ClaimType,@ClaimValue)",
                        param: new
                        {
                            Id = Guid.NewGuid().ToString("D"),
                            UserId = user.Id,
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value
                        },
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static async Task RemoveClaimAsync(this IDbContext context, IdentityUser user, Claim claim)
        {
            using (var ctx = context.BeginTransaction())
            {
                await ctx.Connection.ExecuteAsync(
                    sql: @"DELETE FROM AspNetUserClaims WHERE UserId = @UserId AND ClaimType = @ClaimType AND ClaimValue = @ClaimValue",
                    param: new
                    {
                        UserId = user.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static async Task<IList<UserLoginInfo>> GetLoginsAsync(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return (await ctx.Connection.QueryAsync<UserLoginInfo>(
                        sql: @"SELECT LoginProvider, ProviderKey FROM AspNetUserLogins WHERE UserId = @Id ORDER BY LoginProvider",
                        param: new { user.Id },
                        transaction: ctx.Transaction)).ToList();
        }

        public static async Task AddLoginAsync(this IDbContext context, IdentityUser user, UserLoginInfo login)
        {
            var param = new
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey
            };

            using (var ctx = context.BeginTransaction())
            {
                var id = (await ctx.Connection.QueryAsync<string>(
                        sql: @"SELECT UserId FROM AspNetUserLogins WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey AND UserId = @UserId",
                        param: param,
                        transaction: ctx.Transaction)).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    await ctx.Connection.ExecuteAsync(
                        sql: @"INSERT INTO AspNetUserLogins(LoginProvider,ProviderKey,UserId) VALUES(@LoginProvider,@ProviderKey,@UserId)",
                        param: param,
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static async Task RemoveLoginAsync(this IDbContext context, IdentityUser user, UserLoginInfo login)
        {
            using (var ctx = context.BeginTransaction())
            {
                await ctx.Connection.ExecuteAsync(
                        sql: @"DELETE FROM AspNetUserLogins WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey AND UserId = @UserId",
                        param: new
                        {
                            UserId = user.Id,
                            LoginProvider = login.LoginProvider,
                            ProviderKey = login.ProviderKey
                        },
                        transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static async Task<bool> IsInRoleAsync(this IDbContext context, IdentityUser user, string roleName)
        {
            string resultRoleName;
            using (var ctx = context.Open())
                resultRoleName = (await ctx.Connection.QueryAsync<string>(
                        sql: @"SELECT Name
  FROM AspNetRoles
 WHERE Id IN (SELECT RoleId
			    FROM AspNetUserRoles
			   WHERE UserId = @Id)
   AND Name = @roleName",
                        param: new { user.Id, roleName },
                        transaction: ctx.Transaction)).FirstOrDefault();

            return !string.IsNullOrWhiteSpace(resultRoleName);
        }

        public static async Task<IList<string>> GetRolesAsync(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return (await ctx.Connection.QueryAsync<string>(
                        sql: @"SELECT Name
  FROM AspNetRoles
 WHERE Id IN (SELECT RoleId
				FROM AspNetUserRoles
			   WHERE UserId = @Id)
 ORDER BY Name",
                        param: new { user.Id },
                        transaction: ctx.Transaction)).ToList<string>() as IList<string>;
        }

        public static async Task AddToRoleAsync(this IDbContext context, IdentityUser user, string name)
        {
            using (var ctx = context.BeginTransaction())
            {
                IdentityRole role = await context.FindRoleByNameAsync(name);
                if (role == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, name));
                }

                var param = new
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };

                var id = (await ctx.Connection.QueryAsync<string>(
                        sql: @"SELECT UserId FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
                        param: param,
                        transaction: ctx.Transaction)).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    await ctx.Connection.ExecuteAsync(
                        sql: @"INSERT INTO AspNetUserRoles(UserId,RoleId) VALUES(@UserId,@RoleId)",
                        param: param,
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static async Task RemoveFromRoleAsync(this IDbContext context, IdentityUser user, string name)
        {
            using (var ctx = context.BeginTransaction())
            {
                await ctx.Connection.ExecuteAsync(
                        sql: @"DELETE FROM AspNetUserRoles WHERE UserId = @Id AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = @Name)",
                        param: new
                        {
                            user.Id,
                            name
                        },
                        transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static async Task<IdentityUser> FindAsync(this IDbContext context, UserLoginInfo login)
        {
            if (string.IsNullOrWhiteSpace(login.ProviderKey) || string.IsNullOrWhiteSpace(login.LoginProvider))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityUser>(sql: @"SELECT * 
  FROM AspNetUsers
 WHERE Id = (SELECT UserId
               FROM AspNetUserLogins
              WHERE LoginProvider = @LoginProvider 
                AND ProviderKey = @ProviderKey)",
                        param: login,
                        transaction: ctx.Transaction)).SingleOrDefault();
            }
        }

        public static async Task<IdentityUser> FindByEmailAsync(this IDbContext context, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityUser>(
                        sql: "SELECT * FROM AspNetUsers WHERE Email = @email",
                        param: new { email = email.ToLower() },
                        transaction: ctx.Transaction)).SingleOrDefault();
            }
        }

        public static async Task<IdentityUser> FindByNameAsync(this IDbContext context, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityUser>(
                        sql: "SELECT * FROM AspNetUsers WHERE UserName = @userName",
                        param: new { userName = userName.ToLower() },
                        transaction: ctx.Transaction)).SingleOrDefault();
            }
        }

        public static async Task<IdentityUser> GetUserByIdAsync(this IDbContext context, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<IdentityUser>(
                        sql: "SELECT * FROM AspNetUsers WHERE Id = @userId",
                        param: new { userId },
                        transaction: ctx.Transaction)).SingleOrDefault();
            }
        }

        public static IQueryable<IdentityUser> GetUsers(this IDbContext context)
        {
            using (var ctx = context.Open())
                return ctx.Connection.Query<IdentityUser>(
                    sql: "SELECT * FROM AspNetUsers ORDER BY UserName",
                    transaction: ctx.Transaction).AsQueryable();
        }

        //public static void UpdateProperty(this IDbContext context, IdentityUser user, Expression<Func<object>> modelProperty, object value)
        //{
        //    var p = modelProperty.GetPropertyInfo();

        //    if (null == p || p.DeclaringType != user.GetType())
        //        throw new ArgumentException("Invalid property specified.");

        //    var sql = string.Format("UPDATE [dbo].[AspNetUsers] SET [{0}] = @{0} WHERE [Id] = @Id", p.Name);
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@Id", user.Id);
        //    parameters.Add("@" + p.Name, value: value);

        //    using (var ctx = context.BeginTransaction())
        //    {
        //        ctx.Connection.Execute(
        //            sql: sql, 
        //            param: parameters, 
        //            transaction: ctx.Transaction);

        //        ctx.Commit();
        //    }

        //    p.SetValue(user, value);
        //}

        //static PropertyInfo GetPropertyInfo(this Expression<Func<object>> expression)
        //{
        //    MemberExpression memberExp = null;
        //    if (expression.Body.NodeType == ExpressionType.Convert)
        //    {
        //        memberExp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
        //    }
        //    else if (expression.Body.NodeType == ExpressionType.MemberAccess)
        //    {
        //        memberExp = expression.Body as MemberExpression;
        //    }
        //    return ((memberExp != null) ? memberExp.Member : null) as PropertyInfo;
        //}
    }
}

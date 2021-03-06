﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using LL.Repository;
using System.Data.SqlClient;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    internal static class RefreshTokenDb
    {
        public static async Task CreateTokenAsync(this IDbContext context, RefreshToken token)
        {
            if (string.IsNullOrWhiteSpace(token.Id))
            {
                token.Id = Guid.NewGuid().ToString("D");
            }

            using (var ctx = context.BeginTransaction())
            {
                // max 5 active tickets
                await ctx.Connection.ExecuteAsync(@"DELETE FROM AspNetRefreshTokens 
 WHERE NOT Id IN (SELECT TOP 4 Id FROM AspNetRefreshTokens 
                   WHERE UserId = @UserId AND ClientId = @ClientId
				  ORDER BY ExpiresUtc DESC)
   AND UserId = @UserId AND ClientId = @ClientId",
                    param: new { token.UserId, token.ClientId },
                    transaction: ctx.Transaction);

                await ctx.Connection.ExecuteAsync(@"INSERT INTO AspNetRefreshTokens(Id,UserId,ClientId,IssuedUtc,ExpiresUtc)
     VALUES(@Id,@UserId,@ClientId,@IssuedUtc,@ExpiresUtc)",
                    param: token,
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static async Task<RefreshToken> FindTokenByIdAsync(this IDbContext context, string id)
        {
            using (var ctx = context.Open())
                return (await ctx.Connection.QueryAsync<RefreshToken>(@"SELECT * FROM AspNetRefreshTokens WHERE Id = @Id",
                    param: new { id },
                    transaction: ctx.Transaction)).FirstOrDefault();
        }

        public static async Task DeleteTokenAsync(this IDbContext context, string id)
        {
            using (var ctx = context.Open())
                await ctx.Connection.ExecuteAsync(@"DELETE FROM AspNetRefreshTokens WHERE Id = @Id",
                    param: new { id },
                    transaction: ctx.Transaction);
        }

        public static async Task<RefreshTokenClientApp> FindAppByIdAsync(this IDbContext context, string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<RefreshTokenClientApp>("SELECT * FROM AspNetRefreshTokenApps WHERE Id = @id",
                        param: new { id },
                        transaction: ctx.Transaction)).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> ContainsClientByIdAsync(this IDbContext context, string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                using (var ctx = context.Open())
                    return (await ctx.Connection.QueryAsync<string>("SELECT Id FROM AspNetRefreshTokenApps WHERE Id = @id AND Active = 1",
                        param: new { id },
                        transaction: ctx.Transaction)).FirstOrDefault() != null;
            }
            else
            {
                return false;
            }
        }
    }
}

using AngularJSAuthRefreshToken.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public class ClaimsIdentityFactory<TUser, TKey> :
        Microsoft.AspNet.Identity.ClaimsIdentityFactory<TUser, TKey> 
        where TUser : class, IUser<TKey> 
        where TKey : IEquatable<TKey>
    {
        public override async Task<ClaimsIdentity> CreateAsync(UserManager<TUser, TKey> manager, TUser user, string authenticationType)
        {
            var identity = await base.CreateAsync(manager, user, authenticationType);

            IUserEmailStore<TUser, TKey> emailStore;
            IPublicUserStore<TUser, TKey> store = manager as IPublicUserStore<TUser, TKey>;
            if (store != null && null != (emailStore = store.Store as IUserEmailStore<TUser, TKey>))
            {
                var email = await emailStore.GetEmailAsync(user);
                if (null != email)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }


            return identity;
        }
    }
}
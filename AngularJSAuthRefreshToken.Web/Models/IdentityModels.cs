using AngularJSAuthRefreshToken.Web.AspNetIdentity;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public static class ApplicationUserUtility
    {
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this IdentityUser user, UserManager<IdentityUser> manager, string authenticationType = null)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(user, authenticationType ?? DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
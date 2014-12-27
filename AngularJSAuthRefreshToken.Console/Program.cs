using AngularJSAuthRefreshToken.Web.AspNetIdentity;
using LL.Repository;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken
{
    class Program
    {
        static async Task CreateAdminUser(string userName, string mail, string password)
        {
            password = (new PasswordHasher()).HashPassword(password);

            string securityStamp = Guid.NewGuid().ToString("D"), adminRole = "Admin";

            using (var context = new DbContext()as IDbContext)
            using (var roleStore = new RoleStore(context))
            using (var userStore = new UserStore(context))
            {
                var role = await roleStore.FindByNameAsync(adminRole);
                if (null == role)
                {
                    await roleStore.CreateAsync(new IdentityRole { Name = adminRole });
                }

                var user = await userStore.FindByNameAsync(userName);
                if (null == user)
                {
                    user = new IdentityUser
                    {
                        UserName = userName,
                        Confirmed = true,
                        PasswordHash = password,
                        Email = mail,
                        EmailConfirmed = true,
                        SecurityStamp = securityStamp
                    };

                    await userStore.CreateAsync(user);
                }
                else
                {
                    user.PasswordHash = password;
                    user.Email = mail;
                    user.SecurityStamp = securityStamp;

                    await userStore.UpdateAsync(user);
                }

                await userStore.AddToRoleAsync(user, "Admin");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CreateAdminUser(userName: "test@test.com", mail: "test@test.com", password: "P@ssword123").Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}

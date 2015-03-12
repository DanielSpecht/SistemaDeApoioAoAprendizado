using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
[assembly: OwinStartup(typeof(PF2.Startup))]

namespace PF2
{
    public class Startup
    {
        public static Func<UserManager<AppUser>> UserManagerFactory { get; private set; }


        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(
                new CookieAuthenticationOptions {
                    AuthenticationType = "ApplicationCookie",
                    LoginPath = new PathString("/auth/login")
                    //,ExpireTimeSpan = TimeSpan.FromMinutes(5)
                });


           

            // configure the user manager
            /*
            UserManager class is used to manage users e.g. registering new users,
            validating credentials and loading user information.
            It is not concerned with how user information is stored.
            */
            //Factory Pattern so that I can create a new instance of UserManager at the start of each request 
            UserManagerFactory = () =>
            {
                var usermanager = new UserManager<AppUser>(
                    new UserStore<AppUser>(new AppDbContext()));
                // allow alphanumeric characters in username
                usermanager.UserValidator = new UserValidator<AppUser>(usermanager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };


                return usermanager;
            };
        }
    }
}

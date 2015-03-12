using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
namespace PF2
{
        public class AppDbContext : IdentityDbContext<AppUser>
        {
           

            public AppDbContext()
                : base("DefaultConnection", throwIfV1Schema: false)
            {
            }
        }

}

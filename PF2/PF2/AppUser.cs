
using Microsoft.AspNet.Identity.EntityFramework;

namespace PF2
{
    public class AppUser : IdentityUser
    {
        public string Country { get; set; }
    }
}
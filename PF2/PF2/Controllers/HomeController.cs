using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PF2.Models;
namespace PF2.Controllers
{
    [Authorize]
    public class HomeController : AppController
    {
        // GET: Home

       private readonly UserManager<AppUser> userManager;

        public HomeController()
            : this(Startup.UserManagerFactory.Invoke())
        {
        }

        public HomeController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }
        public ActionResult Index()
        {
            var currentUser1 = userManager.FindById(User.Identity.GetUserId());

            ViewBag.Country = currentUser1.Country;
            return View();
        }
    }
}
using lbfox.Models;
using lbfox.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace lbfox.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Index(VincodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "check fields");
                return View(model);
            }

            ApplicationUser user = null;
            if (!User.IsInRole("admin"))
            {
                using (var ctx = new ApplicationDbContext())
                {
                    user = await ctx.Users.SingleAsync(u => u.UserName == User.Identity.Name);
                    if (user?.RemaingPoints >= 2 == false)
                    {
                        ModelState.AddModelError("", "Insufficient points, contact administrator");
                        return View(model);
                    }
                }
            }

            var vincode = model.Vincode.ToUpper();
            var filePath = HostingEnvironment.MapPath($"~/reports/{vincode}.html");

            if (filePath != null)
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    var client = new RestClient("http://antivin.su");
                    var request = new RestRequest(Method.GET) { Resource = "en/app/getfullreportcarfax.php" };
                    request.AddQueryParameter("mode", "login");
                    request.AddQueryParameter("login", "blue1ray1@gmail.com");
                    request.AddQueryParameter("pass", "803188692wehgwehw");
                    request.AddQueryParameter("vin", vincode);
                    IRestResponse result = await client.ExecuteGetTaskAsync(request);
                    var html = result.Content;

                    if (html.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        ModelState.AddModelError("", "invalid vin code");
                        return View(model);
                    }

                    if (fileInfo.Directory?.Exists == false) fileInfo.Directory?.Create();
                    html = html.Replace("</head>", "<base href=\"/\" /></head>");

                    using (var fs = fileInfo.CreateText())
                    {
                        await fs.WriteAsync(html);
                    }
                }

                if (user != null)
                {
                    using (var ctx = new ApplicationDbContext())
                    {
                        ctx.Users.Attach(user);
                        user.RemaingPoints -= 2;

                        ctx.History.Add(new History()
                        {
                            UserId = user.Id,
                            Vin = model.Vincode,
                            DateCreated = DateTime.UtcNow
                        });

                        var test = ctx.History.ToList();

                        await ctx.SaveChangesAsync();
                    }
                }

                model.IsSuccess = true;
                model.ReportName = fileInfo.Name;
            }

            return View(model);
        }

        public PartialViewResult Header(string activeMenu)
        {
            HeaderViewModel model = new HeaderViewModel()
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Menu = new List<MenuItem>()
            };

            if (model.IsAuthenticated)
            {
                var userMgr = HttpContext.GetOwinContext().Get<ApplicationUserManager>();
                var user = userMgr.FindByName(User.Identity.Name);

                model.Points = user.RemaingPoints;
                model.Name = User.Identity.Name;
                model.Menu.Add(new MenuItem()
                {
                    Name = "Car History Report",
                    Link = Url.Action("Index", "Home", null)
                });

                if (User.IsInRole("admin"))
                {
                    model.Menu.Add(new MenuItem()
                    {
                        Name = "Manage Users",
                        Link = Url.Action("Users", "Admin", null)
                    });
                }
            }

            model.Menu?.ForEach(i =>
            {
                if (i.Name == activeMenu) i.Active = true;
            });

            return PartialView("_Header", model);
        }
    }
}
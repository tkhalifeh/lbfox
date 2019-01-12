using lbfox.Models;
using lbfox.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace lbfox.Controllers
{
    public class HomeController : Controller
    {
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
                ViewBag.Error = "invalid";
                return View(model);
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
                        ModelState.AddModelError("invalid_vin_code", "invalid vin code");
                        return View(model);
                    }

                    if (fileInfo.Directory?.Exists == false) fileInfo.Directory?.Create();
                    html = html.Replace("</head>", "<base href=\"/\" /></head>");

                    using (var fs = fileInfo.CreateText())
                    {
                        await fs.WriteAsync(html);
                    }
                }

                ViewBag.Success = true;
                ViewBag.ReportName = fileInfo.Name;
            }

            return View();
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
﻿using lbfox.Attributes;
using lbfox.Models;
using lbfox.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RestSharp;
using System;
using System.Collections.Concurrent;
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
        public static readonly ConcurrentDictionary<string, object> vinsDic = new ConcurrentDictionary<string, object>();
        public static readonly object mutex = new object();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        [CustomAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<JsonResult> VinReport(VincodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "check fields");
                return Json(ProcessModelState());
            }

            var vincode = model.Vincode.ToUpper();
            if (vinsDic.TryGetValue(vincode, out _))
            {
                ModelState.AddModelError("", $"Request for {vincode} is already in progress");
                return Json(ProcessModelState());
            }

            var userId = User.Identity.GetUserId<int>();
            ApplicationUser user = null;

            using (var ctx = new ApplicationDbContext())
            {
                user = await ctx.Users.SingleAsync(u => u.Id == userId);

                if (!User.IsInRole("admin") && user.RemaingPoints < 1)
                {
                    ModelState.AddModelError("", "Insufficient points, contact administrator");
                    return Json(ProcessModelState());
                }
            }

            var filePath = HostingEnvironment.MapPath($"~/reports/{vincode}.html");
            var response = new VinReportResponse();

            vinsDic.TryAdd(vincode, null);

            try
            {
                if (filePath != null)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fromCache = fileInfo.Exists;

                    if (!fileInfo.Exists)
                    {
                        var html = await CarfaxReport(vincode);

                        if (html.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase) < 0)
                        {
                            ModelState.AddModelError("", "invalid vin code");
                            return Json(ProcessModelState());
                        }

                        if (fileInfo.Directory?.Exists == false) fileInfo.Directory?.Create();
                        html = html.Replace("</head>", "<base href=\"/\" /></head>");

                        using (var fs = fileInfo.CreateText())
                        {
                            await fs.WriteAsync(html);
                        }
                    }

                    var remaingPoints = await SubtractPoints(user);

                    response.ReportFile = Url.Content("~/reports/" + fileInfo.Name);
                    response.Success = true;
                    response.RemainingPoints = remaingPoints;
                    
                    await LogHistory(userId, model.Vincode, fromCache);
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                using(var fs = new StreamWriter(Server.MapPath("~/logs/" + $"{DateTime.Now.ToString("ddMMyyyy")}.txt")))
                {
                    await fs.WriteLineAsync(ex.ToString());
                }

                response.ErrorMessage = ex.Message;
                return Json(response);
            }
            finally
            {
                lock (mutex) { vinsDic.TryRemove(vincode, out _); }
            }
        }

        private object ProcessModelState()
        {
            return new
            {
                Success = false,
                Validation = ModelState.Keys.Select(i => new { Key = i, ModelState[i].Errors })
            };
        }

        private static async Task<string> CarfaxReport(string vincode)
        {
            var client = new RestClient("http://antivin.su");
            var request = new RestRequest(Method.GET) { Resource = "en/app/getfullreportcarfax.php" };
            request.AddQueryParameter("mode", "login");
            request.AddQueryParameter("login", "blue1ray1@gmail.com");
            request.AddQueryParameter("pass", "803188692wehgwehw");
            request.AddQueryParameter("vin", vincode);
            IRestResponse result = await client.ExecuteGetTaskAsync(request);
            var html = result.Content;
            return html;
        }

        private async Task<int?> SubtractPoints(ApplicationUser user)
        {
            using (var ctx = new ApplicationDbContext())
            {
                ctx.Users.Attach(user);
                user.RemaingPoints -= 1;
                await ctx.SaveChangesAsync();

                return User.IsInRole("admin") ? (int?)null : user.RemaingPoints;
            }
        }

        private async Task LogHistory(int userId, string vincode, bool fromCache)
        {
            using (var ctx = new ApplicationDbContext())
            {
                ctx.History.Add(new History()
                {
                    UserId = userId,
                    Vin = vincode,
                    DateCreated = DateTime.UtcNow,
                    FromCache = fromCache,

                });

                await ctx.SaveChangesAsync();
            }
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
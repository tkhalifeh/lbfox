﻿using lbfox.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using RestSharp;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace lbfox.Controllers
{
    public class HomeController : Controller
    {
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if ((model.Username == "bobcell" && model.Password == "bobcell123")
                || (model.Username == "mohsen" && model.Password == "mohsen123"))
            {
                var identity = new ClaimsIdentity(
                    DefaultAuthenticationTypes.ApplicationCookie, 
                    ClaimsIdentity.DefaultNameClaimType, 
                    ClaimsIdentity.DefaultRoleClaimType);

                identity.AddClaim(new Claim(ClaimTypes.Name, model.Username));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, model.Username));
                AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
                return RedirectToAction("Form");
            }

            return View();
        }

        [Authorize]
        public ActionResult Form()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Form(VincodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "invalid";
                return View(model);
            }

            var filePath = HostingEnvironment.MapPath($"~/reports/{model.Vincode}.html");
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
                    request.AddQueryParameter("vin", model.Vincode);
                    IRestResponse result = await client.ExecuteGetTaskAsync(request);
                    var html = result.Content;

                    if (html.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        ModelState.AddModelError("invalid_vin_code", "invalid vin code");
                        return View(model);
                    }

                    if (fileInfo.Directory?.Exists == false) fileInfo.Directory?.Create();
                    html = html
                       .Replace("href=\"", "href=\"http://antivin.su/")
                       .Replace("content=\"", "content=\"http://antivin.su/")
                       .Replace("src=\"", "src=\"http://antivin.su/")
                       .Replace("http://antivin.su/carfax/imgHdr.png", "http://carfaxjo.com/carfaxFiles/imgHdr.png");

                    using (var fs = fileInfo.CreateText())
                    {
                        await fs.WriteAsync(html);
                    }
                }

                ViewBag.Success = true;
                ViewBag.ReportName = $"{model.Vincode}.html";
            }

            return View();
        }
    }
}
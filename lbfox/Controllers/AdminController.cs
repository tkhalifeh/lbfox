using System;
using System.Data.Entity.Migrations;
using System.Linq;
using lbfox.Models;
using lbfox.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace lbfox.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        public AdminController()
        {

        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #region props

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        #endregion

        //
        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, RemaingPoints = model.Points };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    model.IsSuccess = true;
                    model.Message = "user created successfully";
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public ActionResult Users()
        {
            using (var ctx = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId<int>();
                return View(new UserGridViewModel() { Users = ctx.Users.Where(u => u.Id != userId).ToList() });
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpPost]
        public async Task<JsonResult> EditPoints(EditPointViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "check input" });
            }

            try
            {
                using (var ctx = new ApplicationDbContext())
                {
                    var user = ctx.Users.Single(u => u.UserName == model.Username);
                    user.RemaingPoints = model.Points;
                    await ctx.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
using System.Web.Mvc;

namespace lbfox.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ContentResult() {Content = "unauthorized"};
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace lbfox
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            if (WebConfigurationManager.AppSettings["httpsEnabled"] == "true") filters.Add(new RequireHttpsAttribute());
        }
    }
}

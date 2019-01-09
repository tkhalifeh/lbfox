using Microsoft.AspNet.Identity;

namespace lbfox.Models
{
    public class AppUser : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }
}
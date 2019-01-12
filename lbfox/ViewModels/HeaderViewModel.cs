using lbfox.Models;
using System.Collections.Generic;

namespace lbfox.ViewModels
{
    public class HeaderViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public List<MenuItem> Menu { get; set; }
    }
}
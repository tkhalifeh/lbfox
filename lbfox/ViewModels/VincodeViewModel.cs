using System.ComponentModel.DataAnnotations;

namespace lbfox.ViewModels
{
    public class VincodeViewModel
    {
        [Required]
        [StringLength(17, MinimumLength = 17)]
        public string Vincode { get; set; }

        public bool IsSuccess { get; set; }
        public string ReportName { get; set; }
        public string RemainingPoints { get; set; }
    }
}
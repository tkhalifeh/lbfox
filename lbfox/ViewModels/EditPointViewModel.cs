using System.ComponentModel.DataAnnotations;

namespace lbfox.ViewModels
{
    public class EditPointViewModel
    {
        [Required]
        public string Username { get; set; }
        [Range(0, int.MaxValue)]
        public int Points { get; set; }
    }
}
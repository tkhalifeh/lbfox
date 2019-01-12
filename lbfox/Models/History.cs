using System;
using System.ComponentModel.DataAnnotations;

namespace lbfox.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Vin { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
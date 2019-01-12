using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lbfox.Models
{
    [Table("History")]
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
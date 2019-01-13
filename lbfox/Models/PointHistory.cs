using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace lbfox.Models
{
    [Table("PointHistory")]
    public class PointHistory
    {
        public int Id { get; set; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
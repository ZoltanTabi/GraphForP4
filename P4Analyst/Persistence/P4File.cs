using System;
using System.ComponentModel.DataAnnotations;

namespace Persistence
{
    public class P4File
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        public byte[] Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string Hash { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}

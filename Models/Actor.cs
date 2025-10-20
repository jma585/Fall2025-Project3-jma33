using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_jma33.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string IMDBLink { get; set; }
        public byte[]? Photo { get; set; }
    }
}

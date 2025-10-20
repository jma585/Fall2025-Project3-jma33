using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_jma33.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string IMDBLink { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public byte[]? Poster { get; set; }
    }
}

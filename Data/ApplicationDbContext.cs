using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_jma33.Models;

namespace Fall2025_Project3_jma33.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Fall2025_Project3_jma33.Models.Movie> Movie { get; set; } = default!;
        public DbSet<Fall2025_Project3_jma33.Models.Actor> Actor { get; set; } = default!;
        public DbSet<Fall2025_Project3_jma33.Models.MovieActors> MovieActors { get; set; } = default!;
    }
}

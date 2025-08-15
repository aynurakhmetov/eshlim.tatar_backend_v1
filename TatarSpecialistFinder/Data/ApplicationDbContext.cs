using Microsoft.EntityFrameworkCore;
using TatarSpecialistFinder.Models;

namespace TatarSpecialistFinder.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ApplicationRequest> ApplicationRequests { get; set; }
    }
}

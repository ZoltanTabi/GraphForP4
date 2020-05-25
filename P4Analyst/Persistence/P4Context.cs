using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class P4Context : DbContext
    {
        public P4Context(DbContextOptions<P4Context> options) : base(options) { }

        public DbSet<P4File> P4Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<P4File>().ToTable("P4File");
        }
    }
}

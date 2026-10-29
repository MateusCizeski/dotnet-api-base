using ApiBase.Repository.Contexts;
using ApiBase.Sample.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiBase.Sample.Data
{
    public class SampleDbContext : Context
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();

        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

        public override void ModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
                // SQLite has no decimal type: stored as REAL so sorting and comparisons work in SQL.
                entity.Property(p => p.Price).HasConversion<double>();
                entity.Property(p => p.CostPrice).HasConversion<double>();
                entity.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace ApiBase.Repository.Contexts
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        public virtual void ModelCreating(ModelBuilder modelBuilder) { }
    }
}

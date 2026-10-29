using Microsoft.EntityFrameworkCore;

namespace ApiBase.Repository.Contexts
{
    /// <summary>
    /// Base EF Core DbContext for ApiBase applications.
    /// Inherit from this class in your application to add DbSets and configure the model.
    /// Override <see cref="ModelCreating"/> instead of <c>OnModelCreating</c> directly
    /// to avoid having to call <c>base.OnModelCreating</c> manually.
    /// The soft delete query filter is applied automatically after your configuration.
    /// </summary>
    /// <example>
    /// public class AppDbContext : Context
    /// {
    ///     public DbSet&lt;Product&gt; Products =&gt; Set&lt;Product&gt;();
    ///
    ///     public AppDbContext(DbContextOptions&lt;AppDbContext&gt; options) : base(options) { }
    ///
    ///     public override void ModelCreating(ModelBuilder modelBuilder)
    ///     {
    ///         modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    ///     }
    /// }
    /// </example>
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplySoftDeleteQueryFilters();
        }

        /// <summary>
        /// Override this method to configure the EF Core model for your application.
        /// Called automatically by <see cref="OnModelCreating"/>.
        /// </summary>
        public virtual void ModelCreating(ModelBuilder modelBuilder) { }
    }
}

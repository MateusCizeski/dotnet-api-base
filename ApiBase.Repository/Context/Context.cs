using Microsoft.EntityFrameworkCore;

namespace ApiBase.Repository.Contexts
{
    /// <summary>
    /// Base EF Core DbContext for ApiBase applications.
    /// Inherit from this class in your application to add DbSets and configure the model.
    /// Override <see cref="ModelCreating"/> instead of <c>OnModelCreating</c> directly
    /// to avoid having to call <c>base.OnModelCreating</c> manually.
    /// </summary>
    /// <example>
    /// public class AppContext : Context
    /// {
    ///     public DbSet&lt;Product&gt; Products { get; set; }
    ///
    ///     public AppContext(DbContextOptions options) : base(options) { }
    ///
    ///     public override void ModelCreating(ModelBuilder modelBuilder)
    ///     {
    ///         modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);
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
        }

        /// <summary>
        /// Override this method to configure the EF Core model for your application.
        /// Called automatically by <see cref="OnModelCreating"/>.
        /// </summary>
        public virtual void ModelCreating(ModelBuilder modelBuilder) { }
    }
}

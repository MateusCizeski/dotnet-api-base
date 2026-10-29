using ApiBase.Controller.Extensions;
using ApiBase.Repository.Extensions;
using ApiBase.Sample.Application;
using ApiBase.Sample.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// DbContext + IRepositoryBase<T> + IUnitOfWork + query options
builder.Services.AddApiBaseCore<SampleDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")),
    queryOptions => queryOptions.MaxPageSize = 50);

// Exceptions → ProblemDetails (400 invalid query, 404 not found, 500 without details)
builder.Services.AddApiBaseExceptionHandler();

builder.Services.AddScoped<IProductApplication, ProductApplication>();

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    db.Database.EnsureCreated();
    SampleData.Seed(db);
}

app.MapControllers();

app.Run();

/// <summary>Visible to WebApplicationFactory in the integration tests.</summary>
public partial class Program { }

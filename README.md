# dotnet-api-base

A modular framework for ASP.NET Core APIs, published as independent NuGet packages. Provides a generic foundation for building reusable APIs following Clean Architecture and DDD-light principles — low coupling, high productivity.

---

## Packages

| Package | Description |
|---|---|
| `ApiBase.Domain` | Pure entities, interfaces and domain contracts |
| `ApiBase.Infra` | Unit of Work, dynamic query engine, EF Core infrastructure |
| `ApiBase.Repository` | Generic repository with soft delete and async support |
| `ApiBase.Application` | Application services with pagination, filtering and logging |
| `ApiBase.Controller` | Generic async controllers with standardized response helpers |

👉 [View all packages on NuGet](https://www.nuget.org/profiles/seu-usuario)

---

## Installation

```bash
dotnet add package ApiBase.Domain
dotnet add package ApiBase.Infra
dotnet add package ApiBase.Repository
dotnet add package ApiBase.Application
dotnet add package ApiBase.Controller
```

---

## Features

- **Generic CRUD** with pagination, dynamic filtering, sorting and field projection.
- **Async-first** — all read and write operations have async counterparts.
- **Soft delete** — entities implementing `ISoftDelete` are automatically filtered instead of physically deleted.
- **Dynamic query engine** — filter, sort and project to typed or dynamic types at runtime via expression trees.
- **Unit of Work** with `CommitAsync` / `RollbackChangesAsync` and structured logging via `ILogger`.
- **Plug-and-play** NuGet packages — install only what you need.

---

## Quick Start

### 1. Define your entity

```csharp
public class Product : EntityGuid
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

Soft delete support (optional):

```csharp
public class Product : EntityGuid, ISoftDelete
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

### 2. Define your view

```csharp
public class ProductView : IdGuidView
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 3. Define your repository

```csharp
public interface IProductRepository : IRepositoryBase<Product> { }

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(AppContext ctx, ILogger<ProductRepository> logger)
        : base(ctx, logger) { }
}
```

### 4. Define your application service

```csharp
public class ProductApplication
    : ApplicationGuid<Product, IProductRepository, ProductView>
{
    public ProductApplication(
        IUnitOfWork uow,
        IProductRepository repo,
        ILogger<ProductApplication> logger)
        : base(uow, repo, logger) { }
}
```

### 5. Define your controller

```csharp
[ApiController]
[Route("api/products")]
public class ProductController
    : GuidController<IProductApplication, ProductView>
{
    public ProductController(
        IProductApplication app,
        ILogger<ProductController> logger)
        : base(app, logger) { }
}
```

### 6. Register in DI

```csharp
builder.Services.AddApiBaseCore(builder.Configuration);
// Optional JWT support:
// builder.Services.AddApiBaseCore(builder.Configuration).AddApiBaseJwt(builder.Configuration);

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductApplication, ProductApplication>();
```

---

## Dynamic Filtering

Send filters via query string:

```
GET /api/products?filter=[{"property":"Name","value":"Apple","operator":"Contains"}]&page=1&limit=10&sort=[{"property":"Price","direction":"asc"}]
```

Supported operators: `Equal`, `NotEqual`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `In`, `InOrNull`, `ContainsAll`.

Field projection:

```
GET /api/products/{id}?fields=Name,Price
```

---

## Architecture

```
ApiBase.Domain        → Entities, interfaces, query models (no dependencies)
ApiBase.Infra         → EF Core, expression trees, dynamic types, UoW
ApiBase.Repository    → RepositoryBase<T>, soft delete, async methods
ApiBase.Application   → ApplicationGuid<T>, pagination, logging
ApiBase.Controller    → GuidController<T>, BaseApiController, response helpers
```

---

## Tech Stack

- .NET 8+ / ASP.NET Core
- Entity Framework Core
- Repository Pattern & Unit of Work
- Clean Architecture / DDD-light
- System.Text.Json
- xUnit (test project)

---

## Running Tests

```bash
dotnet test
```

The `ApiBase.Tests` project covers `ValueConverter`, `FilterExpressionFactory` and `OrderByQuery` with 33 tests.

---

## License

MIT

# dotnet-api-base

[![CI](https://github.com/MateusCizeski/dotnet-api-base/actions/workflows/ci.yml/badge.svg)](https://github.com/MateusCizeski/dotnet-api-base/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/ApiBase.Application.svg)](https://www.nuget.org/profiles/MateusCizeski)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)

A query engine and base layers for ASP.NET Core read APIs, published as five NuGet packages.
Clients filter, sort, page and pick fields through the query string; the library turns that into
expression trees that EF Core translates to SQL, and returns **views** (read models), never the entities.

```
GET /api/products?filter=[{"property":"Category.Name","value":"Fruit"}]&sort=[{"property":"Price","direction":"desc"}]&page=1&limit=10&fields=Id,Name,Price
```

---

## Why this exists

Most business APIs have dozens of list screens that all need the same thing: filters on any column,
sorting, pagination and a smaller payload for grids. Writing a `ListPaged(...)` method per entity
repeats the same code and each copy gets its own bugs. This library writes that code once, with
the security rules in one place (allowed properties, page size limit, safe error messages).

It is meant for the **query side** (CQRS-style). Commands (create, approve, pay...) should go
through your own services and domain model — see [dotnet-sales-commission-control](https://github.com/MateusCizeski/dotnet-sales-commission-control)
for a project that uses ApiBase only for its list endpoints.

---

## Packages

| Package | What it contains | Depends on |
|---|---|---|
| `ApiBase.Domain` | `EntityGuid`, `ISoftDelete`, `IRepositoryBase<T>`, `IUnitOfWork`, query models, `InvalidQueryException`, `NotFoundException` | nothing |
| `ApiBase.Infra` | Query engine (dynamic where/order by, projection, Reflection.Emit types, pagination), `UnitOfWork`, `ApiBaseOptions` | EF Core |
| `ApiBase.Repository` | `RepositoryBase<T>`, `EfRepository<T>`, base `Context`, soft delete query filter, `AddApiBaseCore<TContext>` | Infra |
| `ApiBase.Application` | `ApplicationGuid<TEntity, TRepository, TView>` — the read service | Infra |
| `ApiBase.Controller` | `GuidController<TApp, TView>`, `ApiBaseExceptionHandler` (ProblemDetails), `AddApiBaseJwt` | ASP.NET Core |

Database providers (SQL Server, PostgreSQL, SQLite...) are **not** included: your application installs the one it uses.

```bash
dotnet add package ApiBase.Repository
dotnet add package ApiBase.Application
dotnet add package ApiBase.Controller
```

---

## Quick start

A complete, runnable version of this section is in [`samples/ApiBase.Sample`](samples/ApiBase.Sample) — it is part of the
solution and the integration tests run against it, so the code below cannot drift from the library.

### 1. Entity

Entities can be rich domain objects: protected constructor, private setters.

```csharp
public class Product : EntityGuid, ISoftDelete
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal CostPrice { get; private set; }   // internal: not in the view
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    protected Product() { }                          // for EF Core

    public Product(string name, decimal price, decimal costPrice, Category category) { /* rules */ }
}
```

### 2. View (what the API returns)

Properties are matched **by name** with the entity. A nested view becomes a JOIN.
Only view properties can be returned, filtered or sorted.

```csharp
public class ProductView : IdGuidView
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public CategorySummaryView? Category { get; set; }
}
```

### 3. DbContext

```csharp
public class AppDbContext : Context   // Context applies the soft delete filter
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override void ModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

Already have a DbContext that inherits `DbContext` directly? Call `modelBuilder.ApplySoftDeleteQueryFilters()` in your `OnModelCreating`.

### 4. Application service

The generic `IRepositoryBase<Product>` is registered for you. Write your own repository
(inheriting `RepositoryBase<T>`) only when you need custom queries.

```csharp
public interface IProductApplication : IApplicationGuid<ProductView> { }

public class ProductApplication
    : ApplicationGuid<Product, IRepositoryBase<Product>, ProductView>, IProductApplication
{
    public ProductApplication(IUnitOfWork uow, IRepositoryBase<Product> repository,
        ILogger<ProductApplication> logger, IOptions<ApiBaseOptions> options)
        : base(uow, repository, logger, options) { }

    // Optional: restrict every query (tenant, ownership...)
    // public override Expression<Func<Product, bool>> DefaultFilter() => p => p.TenantId == _tenant.Id;
}
```

### 5. Controller

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : GuidController<IProductApplication, ProductView>
{
    public ProductsController(IProductApplication app, ILogger<ProductsController> logger)
        : base(app, logger) { }
}
```

### 6. Program.cs

```csharp
builder.Services.AddControllers();

builder.Services.AddApiBaseCore<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")),
    queryOptions => queryOptions.MaxPageSize = 100);

builder.Services.AddApiBaseExceptionHandler(o => o.Map<MyDomainException>(StatusCodes.Status400BadRequest));
builder.Services.AddScoped<IProductApplication, ProductApplication>();

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
```

If your infrastructure layer already registers the DbContext, call `AddApiBaseCore<AppDbContext>()` without the first argument.

---

## Query string reference

| Parameter | Format | Example |
|---|---|---|
| `filter` | JSON array (flat or grouped) | `[{"property":"Name","operator":"contains","value":"app"}]` |
| `sort` | JSON array | `[{"property":"Price","direction":"desc"}]` |
| `page` / `limit` | integers (page is 1-based) | `page=2&limit=20` |
| `start` | 0-based offset, used when `page` is not sent | `start=40&limit=20` |
| `fields` | comma list or JSON array | `fields=Id,Name` |

Grouped filters: `[{"filter":"[{...},{...}]","and":true},{"filter":"[...]","and":false}]`.
Each filter also accepts `"not": true` and `"and": false` (OR with the previous one).

| Operator | SQL | Notes |
|---|---|---|
| `equal` (default) / `notEqual` | `=` / `<>` | |
| `greater` / `greaterOrEqual` / `less` / `lessOrEqual` | `>` `>=` `<` `<=` | |
| `contains` / `startswith` / `endswith` | `LIKE` | case-insensitive (both sides lowercased) |
| `in` / `inOrNull` | `IN (...)` / `IN (...) OR IS NULL` | value is a JSON array |
| `containsall` | `LIKE ... AND LIKE ...` | every term must be present |

A filter with an empty value is ignored (an empty search box). Anything else that cannot be applied
returns **400**, it is never skipped silently.

---

## Security

| Rule | Why |
|---|---|
| **Allow list**: filters and sorts can only use properties of the view | Filters run on the entity so EF can translate them. Without the allow list a client could probe a hidden column with `startswith`, letter by letter (OWASP API3 — Broken Object Property Level Authorization). |
| **`MaxPageSize`** (default 100) caps `limit` | `?limit=50000000` would load and serialize millions of rows (OWASP API4 — Unrestricted Resource Consumption). |
| **No `includes` parameter** | The view already decides which navigations are read; letting the client choose would expose any navigation. |
| **Errors as ProblemDetails** | Known errors return 400/404 with a safe message. Anything else returns 500 with a generic title — the exception, SQL and stack trace go only to the log. |
| **`AddApiBaseJwt` requires a 256-bit secret** | Shorter HS256 keys are rejected at startup. Keep the secret in User Secrets / environment variables, never in `appsettings.json`. |

---

## How a list request flows

```
GET /api/products?filter=...&sort=...&page=...
        │
GuidController.GetList          binds QueryParams, passes the CancellationToken
        │
ApplicationGuid.GetAsync        Repository.Get() + DefaultFilter()
        │
GuidQueryHelper.PageAsync
   1. QueryAllowList.Validate   filter/sort properties must exist in TView     → 400
   2. DynamicWhereBuilder       JSON filters → Expression<Func<TEntity,bool>>  → 400 if invalid
   3. OrderByQuery              sort → OrderBy/ThenBy (default: Id asc)
   4. Project().To<TView>()     MemberInit expression (nested views → JOIN)
   5. SelectDynamic(fields)     runtime type with only the requested fields
   6. CountAsync + Skip/Take(limit ≤ MaxPageSize) + ToListAsync
        │
200 { "content": [...], "total": 42 }
```

Everything up to step 6 only builds an `IQueryable`: EF Core sends **one** query for the page and one `COUNT`.

---

## Design decisions & trade-offs

- **Expression trees instead of System.Linq.Dynamic.Core** — no string parsing of user input into code, no extra dependency, and full control over which properties and operators are accepted. The cost is more code to maintain.
- **Reflection.Emit for `fields`** — EF Core can only translate projections into concrete types, so the requested fields become a runtime type (cached per field set, thread-safe). A `Dictionary` would be simpler but would have to be built in memory after loading all columns.
- **Filters on the entity, allow list from the view** — filtering the projected view would hide entity columns naturally, but nested projections do not always translate. Filtering the entity is predictable; the allow list closes the gap.
- **Global query filter for soft delete** — applies to `Include` and to queries written directly on the `DbContext`; a per-method filter in the repository did not.
- **`null` → 404 for reads, exceptions for commands** — reads return `null` and the controller answers 404; commands throw `NotFoundException`, which the handler maps to 404.
- **Read side only** — `GuidController` has no generic POST/PUT/DELETE: writes belong to the domain, where the business rules are.

---

## Running the sample and the tests

```bash
dotnet run --project samples/ApiBase.Sample      # http://localhost:5080/api/products
dotnet test
```

The sample uses SQLite (`apibase-sample.db`, created and seeded on startup) and logs every SQL command,
so you can see the `WHERE`, `ORDER BY`, `LIMIT` and `COUNT` that each request generates.

Tests (xUnit):
- unit tests for the value converter, filter expressions, ordering and the type cache;
- integration tests with SQLite in memory: DI registration, unit of work, soft delete with `Include`, allow list, page size limit;
- HTTP tests with `WebApplicationFactory` over the sample: status codes, ProblemDetails, JSON contract, no internal details on 500.

---

## Publishing

Push a tag (`git tag v3.0.0 && git push origin v3.0.0`): the [release workflow](.github/workflows/release.yml) builds,
tests, packs and publishes with nuget.org **Trusted Publishing** (no API key stored). `publish.sh` is a manual fallback
that reads the key from the `NUGET_API_KEY` environment variable.

See [CHANGELOG.md](CHANGELOG.md) for the breaking changes in 3.0.0.

---

## License

MIT

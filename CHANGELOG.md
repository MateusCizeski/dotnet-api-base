# Changelog

All notable changes to this project are documented here. The project follows [Semantic Versioning](https://semver.org/).

## [3.0.0] — unreleased

Major version: several public contracts changed (see **Breaking changes**).

### Fixed
- `AddApiBaseCore` registered the base `Context` instead of the application's DbContext, so `DbSet<T>` failed with "entity type not in model", and nothing was registered as `DbContext`, so `UnitOfWork` could not be created. It is now `AddApiBaseCore<TContext>()` and the repository and the unit of work share the same scoped instance.
- The open generic `IRepositoryBase<>` pointed to the abstract `RepositoryBase<>`. It now resolves to `EfRepository<T>`.
- `in`, `inOrNull` and `containsall` filters sent through the query string arrived as `JsonElement` and were silently ignored. Values are now unwrapped and converted to a typed list.
- Invalid filters (unknown property, unconvertible value, unknown operator, malformed JSON) were skipped and the API returned unfiltered data. They now throw `InvalidQueryException` → HTTP 400.
- Soft delete was applied only by some repository methods and not to `Include`. It is now a global EF Core query filter.
- `GetAsync` ran synchronous code inside `Task.Run`. The list now uses `CountAsync`/`ToListAsync` end to end, with `CancellationToken`.
- Sorting was case-sensitive while filtering was not.
- `start` was documented but ignored.
- The dynamic type cache could create two types for the same fields under concurrency, and `{Name, Price}` / `{Price, Name}` produced different types.
- `UnitOfWork.Dispose()` disposed the DbContext owned by the DI container.

### Security
- Filters and sorts accept only properties of the view (allow list) — hidden entity columns can no longer be probed.
- `limit` is capped by `ApiBaseOptions.MaxPageSize` (default 100).
- The `includes` query parameter was removed.
- Exceptions no longer reach the client: `ApiBaseExceptionHandler` returns ProblemDetails (400/404 with safe messages, 500 generic).
- `AddApiBaseJwt` rejects secrets shorter than 256 bits.
- `publish.sh` no longer receives the NuGet API key as an argument; releases use GitHub Actions with Trusted Publishing.

### Added
- `samples/ApiBase.Sample`, integration tests (SQLite) and HTTP tests (`WebApplicationFactory`).
- `ApiBaseOptions` (`DefaultPageSize`, `MaxPageSize`), `InvalidQueryException`, `NotFoundException`, `ApiBaseExceptionHandler`, `ApiBaseExceptionOptions.Map<TException>(status)`, `ApplySoftDeleteQueryFilters()`.
- `fields` also accepts a comma list (`fields=Id,Name`).
- CI workflow (build, tests, vulnerable package check) and release workflow.

### Breaking changes
- Target framework: `net10.0` (was `net8.0`).
- `AddApiBaseCore(Action<DbContextOptionsBuilder>)` → `AddApiBaseCore<TContext>(Action<DbContextOptionsBuilder>? = null, Action<ApiBaseOptions>? = null)`. It no longer calls `AddHttpContextAccessor()` / `AddHealthChecks()`.
- `AddApiBaseJwt` moved from `ApiBase.Repository` to `ApiBase.Controller` (`ApiBase.Controller.Extensions`).
- `EntityGuid` is abstract, generates its `Id` in the constructor and has `protected set`. Repositories no longer assign Ids.
- `IdGuidView` no longer inherits `EntityGuid` (it has its own `Id` and implements `IView`).
- `new()` constraint removed from `TEntity` in `ApplicationBase` / `ApplicationGuid`.
- `RepositoryBase<T>` receives `DbContext` (was `Context`); `Get(QueryParams)` was removed.
- `IRepositoryBase<T>` and `IUnitOfWork` no longer implement `IDisposable`; async methods accept `CancellationToken`.
- `IUnitOfWork.BuildCustomFieldsList` was removed: results are serialized as views, so JSON property names now follow the serializer policy (camelCase by default) instead of PascalCase dictionary keys.
- `IApplicationGuid.Get/GetAsync(id)` return `TView?` (`null` when not found) instead of an empty object; `GuidController` answers 404.
- `GuidController` no longer catches exceptions; `BaseApiController.RespondError(Exception)` and `BuildFilteredResponse` were removed; `Extension.FlattenMessage` was removed.
- `QueryParams.Includes` / `GetIncludes()` were removed; `FilterModel.Operator` (enum) is no longer read from JSON (clients send the `operator` string).
- Package dependencies trimmed: `ApiBase.Repository` no longer brings ASP.NET Core or JwtBearer; database providers and Swagger are chosen by the application.

## [2.x]

Initial public versions on .NET 8.

# Xprema Entity Framework Core Migrations

This project contains database migrations for the Xprema Management system. It's designed as a separate project to isolate migrations from the main application code, following ABP.io's approach to database migrations.

## Project Structure

The migration system has been organized into the `dtv` directory (database, tools, versioning), which contains:

```
dtv/
├── Xprema.EntityFrameworkCore.Migrations/   # Class library for migrations
│   ├── DbContexts/                          # Base and module-specific DbContexts
│   ├── DbMigrations/                        # Migration infrastructure
│   └── appsettings.json                     # Configuration for migrations
└── Xprema.EntityFrameworkCore.Migrator/     # Console app for migrations
    ├── Program.cs                           # Migration commands
    └── appsettings.json                     # Configuration for migrator
```

## Architecture

The migration system is built around these key components:

1. **Core DbContext Abstractions**:
   - `IEfCoreDbContext` - Base interface for all EF Core DbContexts
   - `EfCoreDbContextBase<TDbContext>` - Base class for all DbContexts implementing common functionality

2. **Module-Specific Components**:
   - `IModuleDbContext` - Interface for module-specific DbContexts
   - `ModuleDbContextBase<TDbContext>` - Base class for module-specific DbContexts
   - `XpremaManagementDbContext` - Example module-specific DbContext

3. **Migration Infrastructure**:
   - `XpremaMigrationsDbContext` - Combined DbContext for migrations
   - `XpremaMigrationsDbContextFactory` - Factory for creating the migrations DbContext
   - `DbMigrationService` - Service for applying migrations and seeding data

4. **Migrator Application**:
   - `Xprema.EntityFrameworkCore.Migrator` - Console application for migrations and seeding

## How to Use

### Creating a New Module

To add a new module with its own entities:

1. Create a module class to represent your module:

```csharp
public class MyNewModule
{
    // This class can be empty, it's just used as a marker for the module
}
```

2. Create a module-specific DbContext:

```csharp
public class MyNewModuleDbContext : ModuleDbContextBase<MyNewModuleDbContext, MyNewModule>
{
    public DbSet<MyEntity> MyEntities { get; set; } = null!;

    public MyNewModuleDbContext(DbContextOptions<MyNewModuleDbContext> options)
        : base(options)
    {
    }

    public override void ConfigureModuleModel(ModelBuilder modelBuilder)
    {
        // Configure entities for this module
        modelBuilder.Entity<MyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Additional configuration
        });
    }
}
```

3. Register your module's DbContext:

Add an extension method to the `XpremaDbContexts` class:

```csharp
public static IServiceCollection AddMyNewModuleDbContext(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    services.AddDbContext<MyNewModuleDbContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Use the migrations assembly for all modules
            sqlOptions.MigrationsAssembly(typeof(XpremaMigrationsDbContext).Assembly.GetName().Name);
        });
    });
    
    return services;
}
```

4. Register your module in the migrations DbContext:

Update the `RegisterModuleDbContextTypes` method in `XpremaMigrationsDbContext`:

```csharp
protected virtual void RegisterModuleDbContextTypes()
{
    // Register existing module contexts
    ModuleDbContextTypes.Add(typeof(XpremaManagementDbContext));
    
    // Register your new module
    ModuleDbContextTypes.Add(typeof(MyNewModuleDbContext));
}
```

### Creating Migrations

To create a new migration:

```bash
dotnet ef migrations add [MigrationName] --project dtv/Xprema.EntityFrameworkCore.Migrations --startup-project dtv/Xprema.EntityFrameworkCore.Migrator
```

### Applying Migrations

You can apply migrations in several ways:

1. **Using the migrator console app**:

```bash
cd dtv/Xprema.EntityFrameworkCore.Migrator
dotnet run -- migrate
```

2. **Seeding initial data**:

```bash
cd dtv/Xprema.EntityFrameworkCore.Migrator
dotnet run -- seed
```

3. **Programmatically in your application**:

```csharp
// In Startup.cs or Program.cs
using var scope = app.Services.CreateScope();
var migrationService = scope.ServiceProvider.GetRequiredService<DbMigrationService>();
await migrationService.MigrateAsync();
```

## Best Practices

1. Keep entity definitions and configurations in their respective modules
2. Use module-specific DbContexts for accessing data within module boundaries
3. Use the migrations DbContext only for migrations, not for runtime data access
4. Add data seeding logic in your module's data seed classes
5. Run the migrator as an init container in containerized deployments
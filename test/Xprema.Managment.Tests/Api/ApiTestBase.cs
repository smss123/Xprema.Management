using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.EntityFrameworkCore;
using Xprema.Managment.HttpApi.Host;

namespace Xprema.Managment.Tests.Api;

/// <summary>
/// Base class for API integration tests
/// </summary>
public abstract class ApiTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ManagmentDbContext DbContext;

    protected ApiTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the real database with in-memory database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ManagmentDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ManagmentDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                    
                    // Add additional test services
                    ConfigureServices(services);
                    
                    // Create a new service provider
                    var serviceProvider = services.BuildServiceProvider();
                    
                    // Create a scope to get scoped services
                    using var scope = serviceProvider.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ManagmentDbContext>();
                    
                    // Ensure database is created
                    db.Database.EnsureCreated();
                    
                    // Seed the database with test data
                    SeedDatabase(db);
                });
            });

        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        DbContext = Factory.Services.GetRequiredService<ManagmentDbContext>();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override in derived classes to configure additional services
    }
    
    protected virtual void SeedDatabase(ManagmentDbContext context)
    {
        // Override in derived classes to seed the database with test data
    }
    
    protected virtual StringContent CreateJsonContent(object content)
    {
        return new StringContent(
            JsonSerializer.Serialize(content),
            Encoding.UTF8,
            "application/json");
    }
    
    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        Factory.Dispose();
        Client.Dispose();
    }
} 
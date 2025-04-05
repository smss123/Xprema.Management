using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Xprema.Managment.Application;
using Xprema.Managment.EntityFrameworkCore;
using Xprema.Managment.HttpApi.Host.DbMigrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Xprema Management API", Version = "v1" });
    c.EnableAnnotations();
    c.CustomSchemaIds(type => type.FullName);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure database
var connectionString = builder.Configuration.GetConnectionString("Default") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=XpremaManagement;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddEntityFrameworkCore(connectionString);

// Add application services
builder.Services.AddApplicationServices();

// Add migration service
builder.Services.AddTransient<DbMigrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xprema Management API v1"));
    
    // Apply migrations in development environment
    using (var scope = app.Services.CreateScope())
    {
        var migrationService = scope.ServiceProvider.GetRequiredService<DbMigrationService>();
        await migrationService.MigrateAsync();
        await migrationService.SeedAsync();
    }
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run(); 
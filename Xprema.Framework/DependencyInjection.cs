using Xprema.Framework.Entities.Permission;

// ... existing code ...
// In the AddXpremaFramework method, after the other service registrations
// services.AddScoped<IPermissionManager, PermissionManager>();
// services.AddScoped<IAuthorizationService, AuthorizationService>();
// services.AddScoped<IPermissionDefinitionContext, PermissionDefinitionContext>();
//
// // Register all permission providers
// var permissionProviders = AppDomain.CurrentDomain.GetAssemblies()
//     .SelectMany(a => a.GetTypes())
//     .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(PermissionProvider)))
//     .ToList();
//
// foreach (var providerType in permissionProviders)
// {
//     services.AddScoped(typeof(PermissionProvider), providerType);
// }
// ... existing code ...

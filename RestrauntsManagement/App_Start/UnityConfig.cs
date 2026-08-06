using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Implementations;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Web.Http;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;

namespace DotNetRestaurantManagement
{
    // Configures dependency injection for the application using Unity.
    public static class UnityConfig
    {
        // Registers all application dependencies and configures Unity
        // as the dependency resolver for ASP.NET Web API.
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            // Registers RestaurantDbContext with a hierarchical lifetime,
            // creating one instance per request scope.
            container.RegisterType<RestaurantDbContext>(new HierarchicalLifetimeManager());
            // Registers IUserRepository with its concrete implementation.
            container.RegisterType<IUserRepository, UserRepository>(new HierarchicalLifetimeManager());
            // Registers IPasswordHasher with a singleton lifetime because
            // the password hasher is stateless and can be safely reused.
            container.RegisterType<IPasswordHasher, PasswordHasher>(new ContainerControlledLifetimeManager());
            // Registers IAuthService with its concrete implementation.
            container.RegisterType<IAuthService, AuthService>(new HierarchicalLifetimeManager());
            container.RegisterType<IRefreshTokenRepository, RefreshTokenRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IJwtService, JwtService>();
            // Sets Unity as the dependency resolver for Web API.
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}

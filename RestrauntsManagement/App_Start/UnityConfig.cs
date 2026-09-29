using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Repositories.Implementations;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services;
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
            container.RegisterType<IUserRepository, UserRepository>();
            // Registers IAuthService with its concrete implementation.
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IAddressService, AddressService>();
            container.RegisterType<IAddressRepository, AddressRepository>();
            // Registers IRefreshTokenRepository with its concrete implementation.
            container.RegisterType<IRefreshTokenRepository, RefreshTokenRepository>();
            // Registers RestaurantService
            container.RegisterType<IRestaurantService, RestaurantService>();
            // Registers Restaurants Repositories
            container.RegisterType<IRestaurantRepository, RestaurantRepository>();
            // Registers IOrderService with its concrete implementation.
            container.RegisterType<IOrderService, OrderService>();
            // Registers IOrderRepository with its concrete implementation.
            container.RegisterType<IOrderRepository, OrderRepository>();
            // Registers IOwnerOrderService with its concrete implementation.
            container.RegisterType<IOwnerOrderService, OwnerOrderService>();
            // Registers IOwnerOrderRepository with its concrete implementation.
            container.RegisterType<IOwnerOrderRepository, OwnerOrderRepository>();
            container.RegisterType<IReportRepository, ReportRepository>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<IGenerateReportService, GenerateReportService>();
            // Sets Unity as the dependency resolver for Web API.
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}

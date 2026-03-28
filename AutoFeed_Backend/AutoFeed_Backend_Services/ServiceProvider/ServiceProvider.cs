using Microsoft.Extensions.DependencyInjection;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Services;
using AutoFeed_Backend_Repositories.UnitOfWork;
using Azure.Core;

namespace AutoFeed_Backend_Services.ServiceProvider;

public static class ServiceProvider
{
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }

    public static IServiceCollection AddChickenServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILargeChickenService, LargeChickenService>();
        return services;
    }

    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    public static IServiceCollection AddScheduleServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IScheduleService, ScheduleService>();
        return services;
    }

    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFoodService, FoodService>();
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }

    public static IServiceCollection AddIoTDeviceServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIoTDeviceService, IoTDeviceService>();
        return services;
    }

    public static IServiceCollection AddRequestServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRequestService, RequestService>();
        return services;
    }

    public static IServiceCollection AddBarnServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBarnService, BarnService>();
        return services;
    }

    public static IServiceCollection AddChickenBarnService(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IChickenBarnService, ChickenBarnService>();
        return services;
    }

    public static IServiceCollection AddFlockServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFlockService, FlockService>();
        return services;
    }
  

    public static IServiceCollection AddServiceProvider(this IServiceCollection services)
    {
        services.AddTaskServices();
        services.AddChickenServices();
        services.AddUserServices();
        services.AddScheduleServices();
        services.AddInventoryServices();
        services.AddIoTDeviceServices();
        services.AddRequestServices();
        services.AddBarnServices();
        services.AddChickenBarnService();
        services.AddFlockServices();

        return services;
    }
}
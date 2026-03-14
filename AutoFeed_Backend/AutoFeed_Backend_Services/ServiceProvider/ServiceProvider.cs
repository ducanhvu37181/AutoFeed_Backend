using Microsoft.Extensions.DependencyInjection;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Services;
using AutoFeed_Backend_Repositories.Repositories;

namespace AutoFeed_Backend_Services.ServiceProvider;

public static class ServiceProvider
{
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        services.AddScoped<TaskRepository>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}

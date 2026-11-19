using UserService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUserApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, Services.UserService>();

            return services;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Domain;
using EventService.Application.Interfaces;
using EventService.Application.Repository.Interface;
using EventService.Domain;
using EventService.Infrastructure.DataAccess;
using EventService.Infrastructure.Repository;

namespace EventService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEventRepository, EventRepository>();
            //services.AddScoped<IBookingRepository, BookingRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}

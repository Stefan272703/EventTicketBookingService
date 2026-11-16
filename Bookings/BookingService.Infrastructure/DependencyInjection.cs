using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingService.Application.Repository.Interface;
using BookingService.Infrastructure.DataAccess;
using BookingService.Infrastructure.Repository;

namespace BookingService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            //services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}

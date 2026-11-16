using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingApplication(this IServiceCollection services)
        {

            //services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();
            services.AddHostedService<BookingBackgroundService>();
            //services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}

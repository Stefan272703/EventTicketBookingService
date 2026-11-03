using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketBookingService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingBackgroundService>();

            return services;
        }
    }
}

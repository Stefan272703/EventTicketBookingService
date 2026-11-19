using EventService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketBookingService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEventApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService.Application.Services.EventService> ();

            return services;
        }
    }
}

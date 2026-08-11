
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Middlewares;
using EventTicketBookingService.Services;

namespace EventTicketBookingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IEventService, EventService>();
            builder.Services.AddSingleton<IBookingService, BookingService>();
            builder.Services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();
            builder.Services.AddHostedService<BookingBackgroundService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapSwagger();
                app.MapSwaggerUI();
            }
            
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

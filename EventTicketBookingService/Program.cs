using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Middlewares;
using EventTicketBookingService.Infrastructure.Repository;
using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EventTicketBookingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers().AddJsonOptions(
                options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddHostedService<BookingBackgroundService>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

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

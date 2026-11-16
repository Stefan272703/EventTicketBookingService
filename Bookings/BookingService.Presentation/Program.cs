using BookingService.Infrastructure.DataAccess;
using BuildingBlocks.Authentication;
using BookingService.Infrastructure;
using BuildingBlocks.GlobalExceptionMiddleware.Middlewares;
using BuildingBlocks.Swagger;
using BookingService.Application;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Общая настройка. Swagger UI
            builder.Services.AddPresentation();
            // Общая настройка. Настройка JWT-аутентификации и авторизации
            builder.Services.AddJwtAuthentication(builder.Configuration);
            // Общая настройка. Настройка конфигурирования JWT
            builder.Services.AddJwtConfigure(builder.Configuration);

            // Настройка под Booking
            builder.Services.AddBookingInfrastructure(builder.Configuration);
            builder.Services.AddBookingApplication();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

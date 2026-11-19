using BuildingBlocks.Authentication;
using BuildingBlocks.Domain;
using BuildingBlocks.GlobalExceptionMiddleware.Middlewares;
using BuildingBlocks.Swagger;
using EventService.Infrastructure;
using EventService.Infrastructure.DataAccess;
using EventService.Infrastructure.Workers;
using EventTicketBookingService.Application;
using Microsoft.EntityFrameworkCore;

namespace EventService.Presentation
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

            // Настройка для Event
            builder.Services.AddEventInfrastructure(builder.Configuration);
            builder.Services.AddEventApplication();

            // Настройка Kafka и подписчика
            builder.Services.Configure<KafkaConsumerOptions>(builder.Configuration.GetSection(KafkaConsumerOptions.SectionName));
            builder.Services.AddHostedService<EventConsumerWorker>();

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

using BuildingBlocks.Authentication;
using BuildingBlocks.GlobalExceptionMiddleware.Middlewares;
using BuildingBlocks.Swagger;
using Microsoft.EntityFrameworkCore;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.DataAccess;

namespace UserService.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Общая настройка. Swagger UI
            builder.Services.AddPresentation();
            // Общая настройка. Настройка JWT-аутентификаици и авторизации
            builder.Services.AddJwtAuthentication(builder.Configuration);
            // Общая настройка. Настройка конфигурирования JWT
            builder.Services.AddJwtConfigure(builder.Configuration);

            // Настройка под User
            builder.Services.AddUserInfrastructure(builder.Configuration);
            builder.Services.AddUserApplication();
            builder.Services.AddUserSecurity(builder.Configuration);

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

            // Глобальный обработчик ошибок из общих настроек
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

using EventTicketBookingService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketBookingService.Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Название таблицы
            builder.ToTable("users");
            // Первичный ключ
            builder.HasKey(u => u.Id);
            // Сама БД сгенерирует Id
            builder.Property(u => u.Id)
                .HasColumnName("id");
            // Login обязателен и имеет размер в 50 символов
            builder.Property(u => u.Login)
                .HasColumnName("login")
                .HasMaxLength(50)
                .IsRequired();
            // Уникальный индекс на логин (два пользователя с одинаковым логином невозможны)
            builder.HasIndex(u => u.Login)
                .IsUnique();
            // Колонка с хешем пароля
            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(100) // Хеш SHA-256 в hex это 64 символа, оставляем запас
                .IsRequired();
            // Роль храним как строку 
            builder.Property(u => u.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}

using BookingService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // Название таблицы
            builder.ToTable("bookings");
            // Первичный ключ
            builder.HasKey(b => b.Id);
            // Сама БД сгенерирует Id
            builder.Property(b => b.Id)
                .HasColumnName("id");
            // EventId обязательно
            builder.Property(b => b.EventId)
                .HasColumnName("event_id")
                .IsRequired();
            // UserId обязательно
            builder.Property(b => b.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            // Status обязателен, храним как строку БД, длина 20 символов
            builder.Property(b => b.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            // CreatedAt обязателен
            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            // ProcessedAt обязателен
            builder.Property(b => b.ProcessedAt)
                .HasColumnName("processed_at");
        }
    }
}

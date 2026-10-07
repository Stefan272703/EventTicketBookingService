using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketBookingService.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // Название таблицы
            builder.ToTable("bookings");
            // Первичный ключ
            builder.HasKey(b => b.Id);
            // Само приложение сгенерирует Id
            builder.Property(b => b.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
            // EventId обязательно
            builder.Property(b => b.EventId)
                .HasColumnName("event_id")
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

            // Один-ко-многим
            // У одного события может быть много броней, но у брони только одно событие.
            // Внешний ключ - Id события.
            // Удаление происходит каскадно (Удалив событие, удалятся и брони, относящиеся к данному событию)
            builder.HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

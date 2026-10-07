using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketBookingService.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            // Название таблицы
            builder.ToTable("events");
            // Первичный ключ
            builder.HasKey(e => e.Id);
            // Само приложение сгенерирует Id
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
            // Title обязателен и имеет размер в 200 символов
            builder.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(200);
            // Description иммет максимальную длину в 2000 символов
            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);
            // Начало события обязательно
            builder.Property(e => e.StartAt)
                .HasColumnName("start_at")
                .IsRequired();
            // Конец события обязателен
            builder.Property(e => e.EndAt)
                .HasColumnName("end_at")
                .IsRequired();
            // Количество мест обязательно
            builder.Property(e => e.TotalSeats)
                .HasColumnName("total_seats")
                .IsRequired();
            // Количество доступных мест обязательно
            builder.Property(e => e.AvailableSeats)
                .HasColumnName("available_seats")
                .IsRequired();

            // Один-ко-многим.
            // У одного события может быть много броней, но у брони только одно событие.
            // Внешний ключ - Id события 
            // Удаление происходит каскадно (Удалив событие, удалятся и брони, относящиеся к данному событию)
            builder.HasMany(e => e.Bookings)
                .WithOne(b => b.Event)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

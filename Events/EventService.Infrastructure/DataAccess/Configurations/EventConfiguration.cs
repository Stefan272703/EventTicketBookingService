using EventService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            // Название таблицы
            builder.ToTable("events");
            // Первичный ключ
            builder.HasKey(e => e.Id);
            // Сама БД сгенерирует Id
            builder.Property(e => e.Id)
                .HasColumnName("id");
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
        }
    }
}

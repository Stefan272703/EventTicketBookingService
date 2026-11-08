using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Domain.Models
{
    public class Booking
    {
        // Уникальный идентификатор брони
        public int Id { get; set; }

        // Идентификатор события, к которому относится бронь
        public int EventId { get; set; }

        // Текущий статус брони
        public BookingStatus Status { get; set; }

        // Дата и время создания брони;
        public DateTime CreatedAt { get; set; }

        // Дата и время обработки брони
        public DateTime? ProcessedAt { get; set; }

        public int UserId { get; set; }

        // Навигационное свойство
        public Event? Event { get; set; }

        public Booking()
        {

        }

        private Booking(int eventId, int userId, DateTime createdAt)
        {
            if (eventId <= 0)
                throw new ArgumentException("EventId должен быть положительным", nameof(eventId));

            EventId = eventId;
            UserId = userId;
            Status = BookingStatus.Pending;
            CreatedAt = createdAt;
        }

        public static Booking Create(int eventId, int userId, DateTime createdAt) => new Booking(eventId, userId, createdAt);

        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new ValidationException("Повторная отмена заявки брони невозможна. Бронь уже отменена");
            if (Status == BookingStatus.Rejected)
                throw new ValidationException("Нельяз отменить отклоненную бронь");

            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}

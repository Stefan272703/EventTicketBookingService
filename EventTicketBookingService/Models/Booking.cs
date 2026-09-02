using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class Booking
    {
        // Уникальный идентификатор брони
        public int Id { get; set; }

        // Идентификатор события, к которому относится бронь
        public int EventId { get; set; }

        // Текущий статус брони
        [Required(ErrorMessage = "Status обязательное для заполнения")]
        public BookingStatus Status { get; set; }

        // Дата и время создания брони;
        [Required(ErrorMessage = "CreatedAt обязательное для заполнения")]
        public DateTime CreatedAt { get; set; }

        // Дата и время обработки брони
        public DateTime? ProcessedAt { get; set; } = null;
    }
}

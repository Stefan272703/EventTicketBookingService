using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class Booking
    {
        // Уникальный идентификатор брони
        public Guid Id { get; set; }

        // Идентификатор события, к которому относится бронь
        public Guid EventId { get; set; }

        // Текущий статус брони
        [Required(ErrorMessage ="Status обязательное для заполнения")]
        public BookingStatus Status { get; set; }

        // Дата и время создания брони;
        [Required(ErrorMessage = "CreatedAt обязательное для заполнения")]
        public DateTime CreatedAt { get; set; }

        // Дата и время обработки брони
        [Required(ErrorMessage = "ProcessedAt обязательное для заполнения")]
        public DateTime ProcessedAt { get; set; }
    }
}

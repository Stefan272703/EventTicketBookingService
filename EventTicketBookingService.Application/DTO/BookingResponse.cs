using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.DTO
{
    public class BookingResponse
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

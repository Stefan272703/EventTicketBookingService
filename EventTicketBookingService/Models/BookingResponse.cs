using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class BookingResponse
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

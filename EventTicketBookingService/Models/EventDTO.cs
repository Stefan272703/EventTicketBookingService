using EventTicketBookingService.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class EventDTO
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; } = string.Empty;

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }
    }
}

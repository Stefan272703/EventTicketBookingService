using EventTicketBookingService.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    [NotAfterStartAtTime]
    public class EventInfo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title обязательное для заполнения")]
        public string? Title { get; set; }

        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "StartAt обязательное для заполнения")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "EndAt обязательное для заполнения")]
        public DateTime EndAt { get; set; }

        [Required(ErrorMessage = "TotalSeats обязательное для заполнения")]
        public int? TotalSeats { get; set; }

        public int AvailableSeats { get; set; }
    }
}

namespace EventTicketBookingService.Models
{
    public class BookingDTO
    {
        public BookingStatus Status { get; init; } = BookingStatus.Pending;
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

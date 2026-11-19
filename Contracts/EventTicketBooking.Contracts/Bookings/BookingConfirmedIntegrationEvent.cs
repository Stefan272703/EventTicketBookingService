namespace EventTicketBooking.Contracts.Bookings
{
    public record BookingConfirmedIntegrationEvent
    {
        public int EventId { get; init; }
        public int BookingId { get; init; }
        public int UserId {  get; init; }
        public int SeatsCount {  get; init; }
        public DateTime ConfirmedAt { get; init; }
    }
}

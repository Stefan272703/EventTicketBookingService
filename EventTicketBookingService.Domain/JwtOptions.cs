
namespace EventTicketBookingService.Domain
{
    public class JwtOptions
    {
        public const string SectionName = "JWT";

        public string? Secret { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int LifeTimeMinutes { get; set; } = 60;
    }
}

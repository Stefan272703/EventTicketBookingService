
namespace EventTicketBookingService.Domain.Exceptions
{
    public class PastEventBookingException :Exception
    {
        public PastEventBookingException()
        {

        }
        public PastEventBookingException(string message) : base(message)
        {

        }
    }
}

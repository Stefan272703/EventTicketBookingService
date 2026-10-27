using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Domain.Exceptions
{
    public class ResourceNotFoundException : Exception
    {
        public Event? EventDTO { get; set; }

        public ResourceNotFoundException()
        {

        }
        public ResourceNotFoundException(string message) : base(message)
        {

        }
        public ResourceNotFoundException(Event eventDTO, string message) : base(message)
        {
            EventDTO = eventDTO;
        }
        public ResourceNotFoundException(Event eventDTO, string message, Exception inner) : base(message, inner)
        {
            EventDTO = eventDTO;
        }
    }
}

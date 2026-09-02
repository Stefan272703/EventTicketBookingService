using EventTicketBookingService.Models;

namespace EventTicketBookingService.Exceptions
{
    public class ResourceNotFoundException: Exception
    {
        public Event? EventDTO { get; set; }

        // Хотел узнать, нужно ли добавлять BookingResponse в ResourceNotFoundException, как это правильно реализоать?
        //public BookingResponse? BookingResponse { get; set; }

        public ResourceNotFoundException() 
        {
            
        }
        public ResourceNotFoundException(string message) : base(message)
        {

        }
        public ResourceNotFoundException(Event eventDTO, string message):base(message)
        {
            EventDTO = eventDTO;
        }
        public ResourceNotFoundException(Event eventDTO, string message, Exception inner) : base(message, inner) 
        { 
            EventDTO = eventDTO;
        }
    }
}

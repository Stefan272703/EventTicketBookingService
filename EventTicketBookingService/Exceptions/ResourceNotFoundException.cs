using EventTicketBookingService.Models;

namespace EventTicketBookingService.Exceptions
{
    public class ResourceNotFoundException: Exception
    {
        public EventDTO? EventDTO { get; set; }

        // Хотел узнать, нужно ли добавлять BookingResponse в ResourceNotFoundException, как это правильно реализоать?
        //public BookingResponse? BookingResponse { get; set; }

        public ResourceNotFoundException() 
        {
            
        }
        public ResourceNotFoundException(EventDTO eventDTO, string message):base(message)
        {
            EventDTO = eventDTO;
        }
        public ResourceNotFoundException(EventDTO eventDTO, string message, Exception inner) : base(message, inner) 
        { 
            EventDTO = eventDTO;
        }
    }
}

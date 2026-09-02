using EventTicketBookingService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Attributes
{
    public class NotAfterStartAtTimeAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if(value is EventDTO my_event)
            {
                if(my_event.EndAt <= my_event.StartAt)
                {
                    ErrorMessage = "Конец мероприятия должен быть позже начала мероприятия";
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}

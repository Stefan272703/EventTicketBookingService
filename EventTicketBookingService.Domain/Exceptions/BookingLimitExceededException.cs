using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketBookingService.Domain.Exceptions
{
    public class BookingLimitExceededException : Exception
    {
        public BookingLimitExceededException()
        {

        }
        public BookingLimitExceededException(string message) : base(message)
        {

        }
    }
}

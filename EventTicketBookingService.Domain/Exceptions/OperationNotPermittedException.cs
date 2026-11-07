using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketBookingService.Domain.Exceptions
{
    public class OperationNotPermittedException : Exception
    {
        public OperationNotPermittedException()
        {

        }
        public OperationNotPermittedException(string message) : base(message)
        {

        }
    }
}

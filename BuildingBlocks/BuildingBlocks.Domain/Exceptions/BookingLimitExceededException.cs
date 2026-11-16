namespace BuildingBlocks.Domain.Exceptions
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

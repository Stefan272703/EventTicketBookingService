namespace BuildingBlocks.Domain.Exceptions
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

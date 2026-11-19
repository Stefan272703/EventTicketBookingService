using BookingService.Application.DTO;

namespace BookingService.Application.Interfaces
{
    public interface IBookingPublisher
    {
        public Task PublishAsync<T>(string topic,
            string key,
            T integrationEvent,
            CancellationToken ct = default) 
            where T : class;
            
    }
}

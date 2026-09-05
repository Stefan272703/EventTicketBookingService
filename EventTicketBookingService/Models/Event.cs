using EventTicketBookingService.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; } = string.Empty;

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public int TotalSeats { get; set; }

        public int AvailableSeats 
        { 
            get => _availableSeats;
            private set => _availableSeats = value;
        }

        private int _availableSeats;

        public Event()
        {

        }

        public Event(int totalSeats)
        {
            if(totalSeats <= 0)
            {
                throw new ValidationException("Общее количество мест должно быть положительнмы");
            }
            TotalSeats = totalSeats;
            _availableSeats = totalSeats; // При создании равно TotalSeats
        }

        public bool TryReserveSeats(int count = 1)
        {
            if(count <= 0)
            {
                return false;
            }

            int current, updated;
            do
            {
                current = _availableSeats;
                if (current < count)
                {
                    return false;
                }
                updated = current - count;

            } while (Interlocked.CompareExchange(ref _availableSeats, updated, current) != current);

            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            Interlocked.Add(ref _availableSeats, count);
        }
    }
}

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

        public int AvailableSeats { get; private set; }

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
            AvailableSeats = totalSeats; // При создании равно TotalSeats
        }

        public bool TryReserveSeats(int count = 1)
        {
            if(count <= 0)
            {
                return false;
            }
            if(AvailableSeats < count)
            {
                return false;
            }

            AvailableSeats -= count;
            return true;
        }

        // TODO: Добавить возможность для освобождения мест
        public void ReleaseSeats(int count = 1)
        {

        }
    }
}

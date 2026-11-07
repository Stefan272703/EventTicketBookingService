using EventTicketBookingService.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EventTicketBookingService.Domain.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; } = string.Empty;

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public int TotalSeats { get; set; }

        public int AvailableSeats => _availableSeats;

        private int _availableSeats;

        public List<Booking> Bookings { get; set; } = new List<Booking>();

        private Event()
        {
            Title = null!;
        }

        public Event(int totalSeats)
        {
            if (totalSeats <= 0)
            {
                throw new ValidationException("Общее количество мест должно быть положительным");
            }
            TotalSeats = totalSeats;
            _availableSeats = totalSeats; // При создании равно TotalSeats
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0)
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

        public void Update(string title,
            string description,
            DateTime startAt,
            DateTime endAt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Title не может быть пустым");
            if (startAt >= endAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public void EnsureNotPassed(DateTime now)
        {
            if(now >= StartAt)
                throw new PastEventBookingException($"Нельзя забронировать событие c Id: {Id}, оно началось: {StartAt}, а бронь была в {now}");
        }
    }
}

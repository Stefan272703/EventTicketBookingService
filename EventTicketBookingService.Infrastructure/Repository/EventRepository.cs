using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Application.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using EventTicketBookingService.Application.DTO;

namespace EventTicketBookingService.Infrastructure.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;
        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return _context.Events.AddAsync(@event, cancellationToken).AsTask();
        }

        public async Task<Event?> RemoveEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existingEvent == null)
                return null;

            _context.Remove(existingEvent);
            return existingEvent;
        }

        //public async Task<Event?> UpdateEventByIdAsync(int id, EventInfo eventInfo, CancellationToken cancellationToken = default)
        //{
        //    var existingEvent = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        //    if (existingEvent == null)
        //        return null;

        //    existingEvent?.Title = eventInfo.Title;
        //    existingEvent?.Description = eventInfo.Description;
        //    existingEvent?.StartAt = eventInfo.StartAt;
        //    existingEvent?.EndAt = eventInfo.EndAt;

        //    return existingEvent;
        //}

        public async Task<(IReadOnlyList<Event?> items, int totalCount)> GetFilteredEvents(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var filteredEvents = _context.Events.AsQueryable();
            if (!string.IsNullOrEmpty(title))
            {
                filteredEvents = filteredEvents.Where(t => t.Title.ToLower().Contains(title.ToLower()));
            }
            if (from.HasValue)
            {
                filteredEvents = filteredEvents.Where(t => t.StartAt >= from.Value);
            }
            if (to.HasValue)
            {
                filteredEvents = filteredEvents.Where(t => t.EndAt <= to.Value);
            }

            // Общее количество событий
            int totalCount = await filteredEvents.CountAsync(cancellationToken);
            // пагинация фильтрованного списка событий
            var items = await filteredEvents.OrderBy(i => i.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            // Количество элементов на текущей странице
            int pageSizeByIndex = items.Count();

            return (items, totalCount);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
        {
            if (!await _context.Events.AnyAsync(cancellationToken))
                return 1;

            return await _context.Events.MaxAsync(x => x.Id, cancellationToken) + 1;
        }
    }
}

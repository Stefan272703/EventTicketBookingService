using BookingService.Application.DTO;
using BookingService.Application.Interfaces;
using BookingService.Application.Repository.Interface;
using BookingService.Domain.Models;
using BookingService.Infrastructure.DataAccess;
using BookingService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Tests
{
    public class BaseBookingTest
    {
        protected readonly ServiceProvider _serviceProvider;
        protected readonly IServiceScope _scope;
        private readonly AppDbContext _context;
        protected readonly IBookingService _bookingService;

        public BaseBookingTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        protected async Task<List<BookingResponse>> CreateActiveBookingAsync(int userId, int count,int eventId = 1)
        {
            var result = new List<BookingResponse>();
            for(int i = 0; i < count; i++)
            {
                result.Add(await _bookingService.CreateBookingAsync(eventId, userId));
            }
            return result;
        }
    }
}

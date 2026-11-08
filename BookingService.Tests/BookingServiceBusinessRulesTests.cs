using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceBusinessRulesTests : BaseBookingTest
    {
        // 1. Попытка забронировать прошедшее событие
        [Fact]
        public async Task CreateBookingAsync_PastEvent_ThrowsPastEventBookingException()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            // Создаем событие, которое началось вчера
            var pastDate = DateTime.UtcNow.AddDays(-1);
            var eventId = await CreateTestEventAsync(startAt: pastDate);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<PastEventBookingException>(
                () => _bookingService.CreateBookingAsync(eventId, userId)
            );

            Assert.Contains("Нельзя забронировать событие", ex.Message);
        }

        // 2. При достижении лимита активных броней новая бронь не создается
        [Fact]
        public async Task CreateBookingAsync_WhenLimitReached_ThrowsBookingLimitExceededException()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            // Создаем событие с большим количеством мест, чтобы места не кончились
            var eventId = await CreateTestEventAsync(totalSeats: 100);

            // Act - заполняем лимит (10 броней)
            for (int i = 0; i < User.maxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, userId);
            }

            // Assert - 11-я бронь должна упасть
            await Assert.ThrowsAsync<BookingLimitExceededException>(
                () => _bookingService.CreateBookingAsync(eventId, userId)
            );
        }

        // 3. Лимиты разных пользователей не влияют друг на друга
        [Fact]
        public async Task CreateBookingAsync_DifferentUsers_LimitsAreIndependent()
        {
            // Arrange
            var user1Id = await CreateTestUserAsync("user1");
            var user2Id = await CreateTestUserAsync("user2");
            var eventId = await CreateTestEventAsync(totalSeats: 100);

            // Act - User1 заполняет свой лимит
            for (int i = 0; i < User.maxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, user1Id);
            }

            // Проверяем, что User1 больше не может (для надежности)
            await Assert.ThrowsAsync<BookingLimitExceededException>(
                () => _bookingService.CreateBookingAsync(eventId, user1Id)
            );

            // Assert - User2 спокойно создает бронь
            var user2Booking = await _bookingService.CreateBookingAsync(eventId, user2Id);
            Assert.NotNull(user2Booking);
            Assert.Equal(user2Id, user2Booking.UserId);
        }

        // 4. Проверка, что отмененная бронь освобождает слот в лимите
        [Fact]
        public async Task CreateBookingAsync_AfterCancel_AllowsNewBookingWithinLimit()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var eventId = await CreateTestEventAsync(totalSeats: 100);

            // Заполняем лимит
            var bookings = new List<int>();
            for (int i = 0; i < User.maxActiveBookings; i++)
            {
                var b = await _bookingService.CreateBookingAsync(eventId, userId);
                bookings.Add(b.Id);
            }

            // Act - отменяем одну бронь
            await _bookingService.CancelBookingAsync(bookings.First(), userId);

            // Assert - теперь можем создать новую
            var newBooking = await _bookingService.CreateBookingAsync(eventId, userId);
            Assert.NotNull(newBooking);
        }
    }
}

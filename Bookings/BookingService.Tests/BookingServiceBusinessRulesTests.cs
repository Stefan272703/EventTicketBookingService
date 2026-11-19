using BuildingBlocks.Domain.Exceptions;
using BookingService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceBusinessRulesTests : BaseBookingTest
    {
        // 2. При достижении лимита активных броней новая бронь не создается
        [Fact]
        public async Task CreateBookingAsync_WhenLimitReached_ThrowsBookingLimitExceededException()
        {
            // Arrange
            int userId = 1;
            await CreateActiveBookingAsync(userId, count: 10);

            // Act & Assert - 11-я бронь должна упасть
            await Assert.ThrowsAsync<BookingLimitExceededException>(
                () => _bookingService.CreateBookingAsync(eventId: 1, userId)
            );
        }

        // 3. Лимиты разных пользователей не влияют друг на друга
        [Fact]
        public async Task CreateBookingAsync_DifferentUsers_LimitsAreIndependent()
        {
            // Arrange
            int user1 = 1, user2 = 2;
            await CreateActiveBookingAsync(user1, count: 10);

            // Assert
            // Проверяем, что User1 больше не может (для надежности)
            await Assert.ThrowsAsync<BookingLimitExceededException>(
                () => _bookingService.CreateBookingAsync(eventId: 1, user1)
            );

            // Assert - User2 спокойно создает бронь
            var user2Booking = await _bookingService.CreateBookingAsync(eventId: 1, user2);
            Assert.NotNull(user2Booking);
            Assert.Equal(user2, user2Booking.UserId);
        }

        // 4. Проверка, что отмененная бронь освобождает слот в лимите
        [Fact]
        public async Task CreateBookingAsync_AfterCancel_AllowsNewBookingWithinLimit()
        {
            // Arrange
            int userId = 1;
            var bookings = await CreateActiveBookingAsync(userId, count: 10);

            // Заполняем лимит

            // Act - отменяем одну бронь
            await _bookingService.CancelBookingAsync(bookings.First().Id, userId, "User");

            // Assert - теперь можем создать новую
            var newBooking = await _bookingService.CreateBookingAsync(eventId: 1, userId);
            Assert.NotNull(newBooking);
        }
    }
}

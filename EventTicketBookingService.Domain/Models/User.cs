using EventTicketBookingService.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventTicketBookingService.Domain.Models
{
    public class User
    {
        public const int maxActiveBookings = 5;

        public int Id { get; private set; }
        public string? Login { get; private set; }
        public string? PasswordHash {  get; private set; }
        public UserRoles Role { get; private set; }

        public User()
        {

        }

        public User(int id, string login, string passwordHash, UserRoles userRoles)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ValidationException("Логин не может быть пустым");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ValidationException("Хеш пароля не может быть пустым");

            Id = id;
            Login = login;
            PasswordHash = passwordHash;
            Role = userRoles;
        }

        public void EnsureCanCreateBooking(int currentActiveBookings)
        {
            if (currentActiveBookings >= maxActiveBookings)
                throw new BookingLimitExceededException($"Пользователь с Id: {Id} превысил количество создания активных броней");
        }

        public void EnsureCanCancelBooking(Booking booking)
        {
            // Админ может всё, пользователь — только свои брони
            if (Role == UserRoles.Admin) return;
            if (booking.UserId != Id)
                throw new OperationNotPermittedException($"Пользователь с Id: {Id} с ролью {Role} не иммет прав на изменение статуса брони другого пользователя на Cancel");
        }

        public void EnsureCanConfirmBooking()
        {
            if (Role == UserRoles.Admin) return;
                throw new OperationNotPermittedException($"Пользователь с Id: {Id} с ролью {Role} не имеет права на самостоятельное изменение брони в сосотояние Confirm");
        }

    }
}

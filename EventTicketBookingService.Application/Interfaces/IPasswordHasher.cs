using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketBookingService.Application.Interfaces
{
    public interface IPasswordHasher
    {
        // Хеширование пароля
        public string Hash(string password);

        // Проверка на соответствие пароля хешу
        public bool Verify(string password, string passwordHash);
    }
}

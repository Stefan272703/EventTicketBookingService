using EventTicketBookingService.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EventTicketBookingService.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        // Хеширование пароля
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Пароль не может быть пустым");

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // Верификация
        public bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            var computed = Hash(password);

            return computed == passwordHash;
        }
    }
}

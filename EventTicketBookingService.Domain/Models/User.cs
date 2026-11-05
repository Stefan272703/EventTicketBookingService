using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventTicketBookingService.Domain.Models
{
    public class User
    {
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
    }
}

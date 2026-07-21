using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Domain.Models
{
    public class User
    {
        private User()
        {
            Login = null!;
            PasswordHash = null!;
        }

        public User(Guid id, string login, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login cannot be empty.", nameof(login));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

            Id = id;
            Login = login;
            PasswordHash = passwordHash;
            Role = role;
        }

        public Guid Id { get; private set; }
        public string Login { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    }
}

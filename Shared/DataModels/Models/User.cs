using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Hashed password (not plain text)

        public User() { }

        // Constructor for creating a new user (with secure password hashing)
        public User(string username, string password)
        {
            Username = username;
            // Hash the password with the salt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}

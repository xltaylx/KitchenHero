using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Hashed password (not plain text)
        public string PasswordSalt { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenHash { get;  set; } // Optional, for storing hashed refresh token
        public DateTime? RefreshTokenExpiration { get; set; } // Nullable to handle cases without refresh token

        public User() { }

        // Constructor for creating a new user (with secure password hashing)
        public User(string email, string password)
        {
            Email = email;
            // Hash the password with the salt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool IsRefreshTokenValid(string refreshToken)
        {
            // Implement logic to validate the refresh token
            // This might involve comparing the provided refresh token with the stored hashed value (if using RefreshTokenHash)
            // Consider expiration time check on the user's side for improved security

            if (string.IsNullOrEmpty(RefreshTokenHash) || string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            // Implement logic to compare hashed refresh token values (e.g., using your HashRefreshToken method)
            // You can return true if the hashed values match and the refresh token hasn't expired

            return false;
        }

        internal void SetRefreshToken(string refreshToken, string refreshTokenHash) // Internal setter for controlled access
        {
            RefreshToken = refreshToken;
            RefreshTokenHash = refreshTokenHash; // Optional, if using hashing
        }
    }
}


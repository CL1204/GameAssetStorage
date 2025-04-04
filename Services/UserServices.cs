using BCrypt.Net;
using GameAssetStorage.Models;
using GameAssetStorage.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GameAssetStorage.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> Authenticate(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserByUsername(username.ToLower());

                if (user == null || string.IsNullOrEmpty(user.password))
                {
                    _logger.LogWarning("User not found: {username}", username);
                    return null;
                }

                if (!BCrypt.Net.BCrypt.EnhancedVerify(password, user.password))
                {
                    _logger.LogWarning("Invalid password for user: {username}", username);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user");
                throw;
            }
        }

        public async Task<User> Register(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username is required");
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password is required");
                if (password.Length < 8)
                    throw new ArgumentException("Password must be at least 8 characters");

                var existingUser = await _userRepository.GetUserByUsername(username.ToLower());
                if (existingUser != null)
                    throw new ArgumentException("Username already exists");

                var newUser = new User
                {
                    username = username.ToLower(),
                    password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 10),
                };

                await _userRepository.AddUser(newUser);
                return newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                throw;
            }
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _userRepository.GetUserByUsername(username.ToLower());
        }
    }
}

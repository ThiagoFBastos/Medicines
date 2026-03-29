using Medicines.Interfaces;
using Medicines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Services
{
    public class UserService : IUserService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepositoryManager repositoryManager, ILogger<UserService> logger)
        {
            _repositoryManager = repositoryManager;
            _logger = logger;
        }

        public async Task<bool> AddUserAsync(long userId, string username)
        {
            if (await GetUserByUserIdAsync(userId) is not null)
            {
                _logger.LogWarning($"User with userId {userId} already exists.");
                return false;
            }

            var user = new User
            {
                UserId = userId,
                Username = username
            };

            _repositoryManager.UserRepository.AddUser(user);
            await _repositoryManager.SaveAsync();

            return false;
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            var user = await GetUserByUserIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning($"User with userId {userId} not exists.");
                return false;
            }

            _repositoryManager.UserRepository.DeleteUser(user);
            await _repositoryManager.SaveAsync();

            return true;
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _repositoryManager.UserRepository.GetAllUsersAsync();
        }

        public Task<User?> GetUserByUserIdAsync(long userId)
        {
            return _repositoryManager.UserRepository.GetUserByUserIdAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(long userId, string username)
        {
            var user = await GetUserByUserIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning($"User with userId {userId} not exists.");
                return false;
            }

            user.Username = username;

            _repositoryManager.UserRepository.UpdateUser(user);
            await _repositoryManager.SaveAsync();

            return true;
        }
    }
}

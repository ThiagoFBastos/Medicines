using Medicines.Enums;
using Medicines.Interfaces;
using Medicines.Models;
using Medicines.Utils;
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

        public async Task<Result<bool, EUserStatusCode>> AddUserAsync(long userId, string username)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                if (result.Value is not null)
                {
                    _logger.LogWarning($"User with userId {userId} already exists.");
                    return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_ALREADY_EXISTS);
                }

                var user = new User
                {
                    UserId = userId,
                    Username = username
                };

                if (!user.IsValid())
                {
                    return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_DATA_INVALID);
                }

                _repositoryManager.UserRepository.AddUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, EUserStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding user with userId {userId}.");
                return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_ADD_ERROR);
            }
        }

        public async Task<Result<bool, EUserStatusCode>> DeleteUserAsync(long userId)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                var user = result.Value;

                if (user is null)
                {
                    _logger.LogWarning($"User with userId {userId} not exists.");
                    return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_NOT_FOUND);
                }

                _repositoryManager.UserRepository.DeleteUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, EUserStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with userId {userId}.");
                return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_DELETE_ERROR);
            }
        }

        public async Task<Result<IEnumerable<User>, EUserStatusCode>> GetAllUsersAsync()
        {
            try
            {
                return Result<IEnumerable<User>, EUserStatusCode>.Success(await _repositoryManager.UserRepository.GetAllUsersAsync());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users.");
                return Result<IEnumerable<User>, EUserStatusCode>.Failure(EUserStatusCode.USER_LIST_ERROR);
            }
        }

        public async Task<Result<User?, EUserStatusCode>> GetUserByUserIdAsync(long userId)
        {
            try
            {
                var user = await _repositoryManager.UserRepository.GetUserByUserIdAsync(userId);

                if (user is null)
                    return Result<User?, EUserStatusCode>.Failure(EUserStatusCode.USER_NOT_FOUND);

                return Result<User?, EUserStatusCode>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting user with userId {userId}.");
                return Result<User?, EUserStatusCode>.Failure(EUserStatusCode.USER_GET_ERROR);
            }
        }

        public async Task<Result<bool, EUserStatusCode>> UpdateUserAsync(long userId, string username)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                var user = result.Value;

                if (user is null)
                {
                    _logger.LogWarning($"User with userId {userId} not exists.");
                    return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_NOT_FOUND);
                }

                user.Username = username;

                if (!user.IsValid())
                {
                    return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_DATA_INVALID);
                }

                _repositoryManager.UserRepository.UpdateUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, EUserStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with userId {userId}.");
                return Result<bool, EUserStatusCode>.Failure(EUserStatusCode.USER_UPDATE_ERROR);
            }
        }
    }
}

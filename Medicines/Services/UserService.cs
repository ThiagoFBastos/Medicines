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

        public async Task<Result<bool, string>> AddUserAsync(long userId, string username)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                if (result.Value is not null)
                {
                    _logger.LogWarning($"User with userId {userId} already exists.");
                    return Result<bool, string>.Failure($"O usuário com id {userId} já existe");
                }

                var user = new User
                {
                    UserId = userId,
                    Username = username
                };

                if (!user.IsValid())
                {
                    return Result<bool, string>.Failure($"""
                        Os dados do usuário estão inválidos.\n
                        Verifique se o username só contém caracteres alfanuméricos, ponto ou underscore, além disso possui entre 1 e 50 caracteres!
                    """);
                }

                _repositoryManager.UserRepository.AddUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding user with userId {userId}.");
                return Result<bool, string>.Failure($"Ocorreu um erro ao adicionar o usuário com id {userId}");
            }
        }

        public async Task<Result<bool, string>> DeleteUserAsync(long userId)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                var user = result.Value;

                if (user is null)
                {
                    _logger.LogWarning($"User with userId {userId} not exists.");
                    return Result<bool, string>.Failure($"O usuário com id {userId} não existe");
                }

                _repositoryManager.UserRepository.DeleteUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with userId {userId}.");
                return Result<bool, string>.Failure($"Ocorreu um erro ao deletar o usuário com id {userId}");
            }
        }

        public async Task<Result<IEnumerable<User>, string>> GetAllUsersAsync()
        {
            try
            {
                return Result<IEnumerable<User>, string>.Success(await _repositoryManager.UserRepository.GetAllUsersAsync());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users.");
                return Result<IEnumerable<User>, string>.Failure("Ocorreu um erro ao obter todos os usuários");
            }
        }

        public async Task<Result<User?, string>> GetUserByUserIdAsync(long userId)
        {
            try
            {
                var user = await _repositoryManager.UserRepository.GetUserByUserIdAsync(userId);

                if (user is null)
                    return Result<User?, string>.Failure($"Usuário com id {userId} não encontrado");

                return Result<User?, string>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting user with userId {userId}.");
                return Result<User?, string>.Failure($"Ocorreu um erro ao obter o usuário com id {userId}");
            }
        }

        public async Task<Result<bool, string>> UpdateUserAsync(long userId, string username)
        {
            try
            {
                var result = await GetUserByUserIdAsync(userId);

                var user = result.Value;

                if (user is null)
                {
                    _logger.LogWarning($"User with userId {userId} not exists.");
                    return Result<bool, string>.Failure($"O usuário com id {userId} não existe");
                }

                user.Username = username;

                if (!user.IsValid())
                {
                    return Result<bool, string>.Failure($"""
                        Os dados do usuário estão inválidos.\n
                        Verifique se o username só contém caracteres alfanuméricos, ponto ou underscore, além disso possui entre 1 e 50 caracteres!
                    """);
                }

                _repositoryManager.UserRepository.UpdateUser(user);
                await _repositoryManager.SaveAsync();

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with userId {userId}.");
                return Result<bool, string>.Failure($"Ocorreu um erro ao atualizar o usuário com id {userId}");
            }
        }
    }
}

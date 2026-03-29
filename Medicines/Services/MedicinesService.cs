using Medicines.Interfaces;
using Medicines.Models;
using Medicines.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Medicines.Services
{
    public class MedicinesService : IMedicinesService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILogger<MedicinesService> _logger;

        public MedicinesService(IRepositoryManager repositoryManager, ILogger<MedicinesService> logger)
        {
            _repositoryManager = repositoryManager;
            _logger = logger;
        }

        public async Task<Result<bool, string>> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            name = name.ToLower();

            try
            {
                var result = await GetMedicineByNameAsync(name, userId);

                if (result.Value is not null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} already been registered");
                    return Result<bool, string>.Failure($"O remédio {name} já está cadastrado");
                }

                var medicine = new Medicine { Name = name, PillsQuantity = pillsQuantity, UserId = userId, ScheduledTime = scheduledTime };

                _repositoryManager.MedicineRepository.AddMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi cadastrado com {pillsQuantity} comprimidos e horário {scheduledTime}");

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding the medicine {name} of user {userId}");
                return Result<bool, string>.Failure($"Ocorreu um erro ao cadastrar o remédio {name}");
            }
        }

        public async Task<Result<bool, string>> AddMedicinePillsAsync(string name, int pillsQuantity, long userId)
        {
            try
            {
                var result = await GetMedicineByNameAsync(name, userId);

                var medicine = result.Value;

                if (medicine is null)
                {
                    _logger.LogInformation($"The number of pills of the medicine {name} of user {userId} wasn't updated");
                    return Result<bool, string>.Failure($"O número de comprimido do remédio {name} não foi atualizado");
                }

                medicine.PillsQuantity += pillsQuantity;
                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O número de comprimidos do remédio {name} do usuário {userId} foi atualizado para {medicine.PillsQuantity}");

                return Result<bool, string>.Success(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the number of pills of the medicine {name} of user {userId}");
                return Result<bool, string>.Failure($"Ocorreu um erro ao atualizar o número de comprimidos do remédio {name}");
            }
        }

        public async Task<Result<bool, string>> DeleteMedicineAsync(string name, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, string>.Failure($"O remédio {name} não foi encontrado");
                }

                _repositoryManager.MedicineRepository.DeleteMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi excluído");

                return Result<bool, string>.Success(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the medicine {name} of user {userId}");
                return Result<bool, string>.Failure($"Ocorreu um erro ao excluir o remédio {name}");
            }
        }

        public async Task<Result<IEnumerable<Medicine>, string>> GetAllMedicinesAsync(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                var medicines = await _repositoryManager.MedicineRepository
                        .GetAllMedicinesAsync(userId);

                return Result<IEnumerable<Medicine>, string>.Success(medicines.Select(med => new Medicine
                {
                    Id = med.Id,
                    Name = med.Name,
                    PillsQuantity = med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays,
                    ScheduledTime = TimeZoneInfo.ConvertTime(med.ScheduledTime, tz),
                    UserId = med.UserId,
                    RegisteredDate = TimeZoneInfo.ConvertTime(med.RegisteredDate, tz)
                }).AsEnumerable());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicines of user {userId}");
                return Result<IEnumerable<Medicine>, string>.Failure($"Ocorreu um erro ao recuperar os remédios");
            }
        }

        public async Task<Result<Medicine?, string>> GetMedicineByIdAsync(Guid id)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByIdAsync(id);

                if (medicine is not null)
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                    medicine.ScheduledTime = TimeZoneInfo.ConvertTime(medicine.ScheduledTime, tz);
                    medicine.RegisteredDate = TimeZoneInfo.ConvertTime(medicine.RegisteredDate, tz);
                }

                return Result<Medicine?, string>.Success(medicine);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicine with id {id}");
                return Result<Medicine?, string>.Failure($"Ocorreu um erro ao recuperar o remédio");
            }
        }

        public async Task<Result<Medicine?, string>> GetMedicineByNameAsync(string name, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is not null)
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                    medicine.ScheduledTime = TimeZoneInfo.ConvertTime(medicine.ScheduledTime, tz);
                    medicine.RegisteredDate = TimeZoneInfo.ConvertTime(medicine.RegisteredDate, tz);
                }
                else
                {
                    return Result<Medicine?, string>.Failure($"O remédio {name} não foi encontrado");
                }

                return Result<Medicine?, string>.Success(medicine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicine {name} of user {userId}");
                return Result<Medicine?, string>.Failure($"Ocorreu um erro ao recuperar o remédio {name}");
            }
        }

        public async Task<Result<bool, string>> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, string>.Failure($"O remédio {name} não foi encontrado");
                }

                medicine.PillsQuantity = pillsQuantity;
                medicine.ScheduledTime = scheduledTime;

                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi atualizado para {pillsQuantity} comprimidos e horário {scheduledTime}");

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the medicine {name} of user {userId}");
                return Result<bool, string>.Failure($"Ocorreu um erro ao atualizar o remédio {name}");
            }
        }

        public async Task<Result<bool, string>> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, string>.Failure($"O remédio {name} não foi encontrado");
                }

                medicine.ScheduledTime = scheduledTime;

                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O horário do remédio {name} do usuário {userId} foi atualizado para {scheduledTime}");

                return Result<bool, string>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the scheduled time of the medicine {name} of user {userId}");
                return Result<bool, string>.Failure($"Ocorreu um erro ao atualizar o horário do remédio {name}");
            }
        }

        public async Task<Result<IEnumerable<Medicine>, string>> GetMedicinesWithFewPills(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                return Result<IEnumerable<Medicine>, string>.Success(await _repositoryManager.MedicineRepository
                        .FindByCondition(med => med.UserId == userId && med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays <= 20)
                        .Select(med => new Medicine
                        {
                            Id = med.Id,
                            Name = med.Name,
                            PillsQuantity = med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays,
                            ScheduledTime = TimeZoneInfo.ConvertTime(med.ScheduledTime, tz),
                            UserId = med.UserId,
                            RegisteredDate = TimeZoneInfo.ConvertTime(med.RegisteredDate, tz)
                        })
                        .ToListAsync()
                        .ContinueWith(task => task.Result.AsEnumerable()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicines with few pills of user {userId}");
                return Result<IEnumerable<Medicine>, string>.Failure($"Ocorreu um erro ao recuperar os remédios com poucos comprimidos");
            }
        }

        public async Task<Result<IEnumerable<Medicine>, string>> GetMedicinesToTakeTodayAsync(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                return Result<IEnumerable<Medicine>, string>.Success(await _repositoryManager.MedicineRepository
                       .FindByCondition(med => med.UserId == userId && med.ScheduledTime.TimeOfDay >= now.TimeOfDay
                       && (med.ScheduledTime.TimeOfDay - now.TimeOfDay).TotalHours <= 2
                       && (med.ScheduledTime.TimeOfDay - now.TimeOfDay).TotalMinutes % 30 == 0)
                       .Select(med => new Medicine
                       {
                           Id = med.Id,
                           Name = med.Name,
                           PillsQuantity = med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays,
                           ScheduledTime = TimeZoneInfo.ConvertTime(med.ScheduledTime, tz),
                           UserId = med.UserId,
                           RegisteredDate = TimeZoneInfo.ConvertTime(med.RegisteredDate, tz)
                       })
                       .ToListAsync()
                       .ContinueWith(task => task.Result.AsEnumerable()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicines to take today of user {userId}");
                return Result<IEnumerable<Medicine>, string>.Failure($"Ocorreu um erro ao recuperar os remédios para tomar hoje");
            }
        }
    }
}

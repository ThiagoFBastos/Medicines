using Medicines.Enums;
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

        public async Task<Result<bool, EMedicinesStatusCode>> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            try
            {
                var result = await GetMedicineByNameAsync(name, userId);

                if (result.Value is not null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} already exists");
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_ALREADY_EXISTS);
                }

                var medicine = new Medicine { Name = name, PillsQuantity = pillsQuantity, UserId = userId, ScheduledTime = scheduledTime };

                if(!medicine.IsValid())
                {
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_DATA_INVALID);
                }

                _repositoryManager.MedicineRepository.AddMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi cadastrado com {pillsQuantity} comprimidos e horário {scheduledTime:HH:mm}");

                return Result<bool, EMedicinesStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding the medicine {name} of user {userId}");
                return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_ADD_ERROR);
            }
        }

        public async Task<Result<bool, EMedicinesStatusCode>> AddMedicinePillsAsync(string name, int pillsQuantity, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_NOT_FOUND);
                }

                medicine.PillsQuantity += pillsQuantity;

                if(!medicine.IsValid())
                {
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_DATA_INVALID);
                }

                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O número de comprimidos do remédio {name} do usuário {userId} foi atualizado para {medicine.PillsQuantity}");

                return Result<bool, EMedicinesStatusCode>.Success(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the number of pills of the medicine {name} of user {userId}");
                return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_UPDATE_ERROR);
            }
        }

        public async Task<Result<bool, EMedicinesStatusCode>> DeleteMedicineAsync(string name, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_NOT_FOUND);
                }

                _repositoryManager.MedicineRepository.DeleteMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi excluído");

                return Result<bool, EMedicinesStatusCode>.Success(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the medicine {name} of user {userId}");
                return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_DELETE_ERROR);
            }
        }

        public async Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetAllMedicinesAsync(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                var medicines = await _repositoryManager.MedicineRepository
                        .GetAllMedicinesAsync(userId);

                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Success(medicines.Select(med => new Medicine
                {
                    Id = med.Id,
                    Name = med.Name,
                    PillsQuantity = Math.Max(0, med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays),
                    ScheduledTime = TimeZoneInfo.ConvertTime(med.ScheduledTime, tz),
                    UserId = med.UserId,
                    RegisteredDate = TimeZoneInfo.ConvertTime(med.RegisteredDate, tz)
                }).AsEnumerable());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicines of user {userId}");
                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_LIST_ERROR);
            }
        }

        public async Task<Result<Medicine?, EMedicinesStatusCode>> GetMedicineByIdAsync(Guid id)
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

                return Result<Medicine?, EMedicinesStatusCode>.Success(medicine);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicine with id {id}");
                return Result<Medicine?, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_GET_ERROR);
            }
        }

        public async Task<Result<Medicine?, EMedicinesStatusCode>> GetMedicineByNameAsync(string name, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is not null)
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                    medicine.PillsQuantity -= (int)(DateTimeOffset.UtcNow - medicine.RegisteredDate).TotalDays;
                    medicine.PillsQuantity = Math.Max(0, medicine.PillsQuantity);
                    medicine.ScheduledTime = TimeZoneInfo.ConvertTime(medicine.ScheduledTime, tz);
                    medicine.RegisteredDate = TimeZoneInfo.ConvertTime(medicine.RegisteredDate, tz);
                }
                else
                {
                    return Result<Medicine?, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_NOT_FOUND);
                }

                return Result<Medicine?, EMedicinesStatusCode>.Success(medicine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the medicine {name} of user {userId}");
                return Result<Medicine?, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_GET_ERROR);
            }
        }

        public async Task<Result<bool, EMedicinesStatusCode>> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_NOT_FOUND);
                }

                medicine.PillsQuantity = pillsQuantity + (int)(DateTimeOffset.UtcNow - medicine.RegisteredDate).TotalDays;
                medicine.ScheduledTime = scheduledTime;

                if (!medicine.IsValid())
                {
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_DATA_INVALID);
                }

                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O remédio {name} do usuário {userId} foi atualizado para {pillsQuantity} comprimidos e horário {scheduledTime:HH:mm}");

                return Result<bool, EMedicinesStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the medicine {name} of user {userId}");
                return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_UPDATE_ERROR);
            }
        }

        public async Task<Result<bool, EMedicinesStatusCode>> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId)
        {
            try
            {
                var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

                if (medicine is null)
                {
                    _logger.LogInformation($"The medicine {name} of user {userId} wasn't found");
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_NOT_FOUND);
                }

                medicine.ScheduledTime = scheduledTime;

                if (!medicine.IsValid())
                {
                    return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_DATA_INVALID);
                }

                _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
                await _repositoryManager.SaveAsync();

                _logger.LogInformation($"O horário do remédio {name} do usuário {userId} foi atualizado para {scheduledTime:HH:mm}");

                return Result<bool, EMedicinesStatusCode>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the scheduled time of the medicine {name} of user {userId}");
                return Result<bool, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_UPDATE_ERROR);
            }
        }

        public async Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetMedicinesWithFewPills(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Success(await _repositoryManager.MedicineRepository
                        .FindByCondition(med => med.UserId == userId 
                        && med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays <= 20
                        && ((int)now.TimeOfDay.TotalHours) % 6 == 0)
                        .Select(med => new Medicine
                        {
                            Id = med.Id,
                            Name = med.Name,
                            PillsQuantity = Math.Max(0, med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays),
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
                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_LIST_ERROR);
            }
        }

        public async Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetMedicinesToTakeTodayAsync(long userId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

                var now = DateTimeOffset.UtcNow;

                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Success(await _repositoryManager.MedicineRepository
                       .FindByCondition(med => med.UserId == userId && med.ScheduledTime.TimeOfDay >= now.TimeOfDay
                       && (med.ScheduledTime.TimeOfDay - now.TimeOfDay).TotalHours <= 2
                       && (med.ScheduledTime.TimeOfDay - now.TimeOfDay).TotalMinutes % 30 == 0)
                       .Select(med => new Medicine
                       {
                           Id = med.Id,
                           Name = med.Name,
                           PillsQuantity = Math.Max(0, med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays),
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
                return Result<IEnumerable<Medicine>, EMedicinesStatusCode>.Failure(EMedicinesStatusCode.MEDICINE_LIST_ERROR);
            }
        }
    }
}

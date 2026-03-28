using Medicines.Interfaces;
using Medicines.Models;
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

        public async Task<bool> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            name = name.ToLower();

            if (await GetMedicineByNameAsync(name, userId) is not null)
            {
                _logger.LogInformation($"O remédio {name} do usuário {userId} já está cadastrado");
                return false;
            }

            var medicine = new Medicine { Name = name, PillsQuantity = pillsQuantity, UserId = userId, ScheduledTime = scheduledTime };

            _repositoryManager.MedicineRepository.AddMedicine(medicine);
            await _repositoryManager.SaveAsync();

            _logger.LogInformation($"O remédio {name} do usuário {userId} foi cadastrado com {pillsQuantity} comprimidos e horário {scheduledTime}");

            return true;
        }

        public async Task<bool> AddMedicinePillsAsync(string name, int pillsQuantity, long userId)
        {
            var medicine = await GetMedicineByNameAsync(name, userId);

            if (medicine is null)
            {
                _logger.LogInformation($"O número de comprimidos do remédio {name} do usuário {userId} não foi atualizado");
                return false;
            }

            medicine.PillsQuantity += pillsQuantity;
            _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
            await _repositoryManager.SaveAsync();

            _logger.LogInformation($"O número de comprimidos do remédio {name} do usuário {userId} foi atualizado para {medicine.PillsQuantity}");

            return true;
        }

        public async Task<bool> DeleteMedicineAsync(string name, long userId)
        {
            var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

            if (medicine is null)
            {
                _logger.LogInformation($"O remédio {name} do usuário {userId} não foi encontrado");
                return false;
            }

            _repositoryManager.MedicineRepository.DeleteMedicine(medicine);
            await _repositoryManager.SaveAsync();

            _logger.LogInformation($"O remédio {name} do usuário {userId} foi excluído");

            return true;
        }

        public async Task<IEnumerable<Medicine>> GetAllMedicinesAsync(long userId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            var now = DateTimeOffset.UtcNow;

            var medicines = await _repositoryManager.MedicineRepository
                    .GetAllMedicinesAsync(userId);

            return medicines.Select(med => new Medicine
                {
                    Id = med.Id,
                    Name = med.Name,
                    PillsQuantity = med.PillsQuantity - (int)(now - med.RegisteredDate).TotalDays,
                    ScheduledTime = TimeZoneInfo.ConvertTime(med.ScheduledTime, tz),
                    UserId = med.UserId,
                    RegisteredDate = TimeZoneInfo.ConvertTime(med.RegisteredDate, tz)
                }).AsEnumerable();
        }

        public async Task<Medicine?> GetMedicineByIdAsync(Guid id)
        {
            var medicine = await _repositoryManager.MedicineRepository.GetMedicineByIdAsync(id);

            if(medicine is not null)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                medicine.ScheduledTime = TimeZoneInfo.ConvertTime(medicine.ScheduledTime, tz);
                medicine.RegisteredDate = TimeZoneInfo.ConvertTime(medicine.RegisteredDate, tz);
            }

            return medicine;
        }

        public async Task<Medicine?> GetMedicineByNameAsync(string name, long userId)
        {
            var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

            if (medicine is not null)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                medicine.ScheduledTime = TimeZoneInfo.ConvertTime(medicine.ScheduledTime, tz);
                medicine.RegisteredDate = TimeZoneInfo.ConvertTime(medicine.RegisteredDate, tz);
            }

            return medicine;
        }

        public async Task<bool> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId)
        {
            var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

            if (medicine is null)
            {
                _logger.LogInformation($"O remédio {name} do usuário {userId} não foi encontrado");
                return false;
            }

            medicine.PillsQuantity = pillsQuantity;
            medicine.ScheduledTime = scheduledTime;

            _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
            await _repositoryManager.SaveAsync();

            _logger.LogInformation($"O remédio {name} do usuário {userId} foi atualizado para {pillsQuantity} comprimidos e horário {scheduledTime}");

            return true;
        }

        public async Task<bool> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId)
        {
            var medicine = await _repositoryManager.MedicineRepository.GetMedicineByNameAsync(name, userId);

            if (medicine is null)
            {
                _logger.LogInformation($"O remédio {name} do usuário {userId} não foi encontrado");
                return false;
            }

            medicine.ScheduledTime = scheduledTime;

            _repositoryManager.MedicineRepository.UpdateMedicine(medicine);
            await _repositoryManager.SaveAsync();

            _logger.LogInformation($"O horário do remédio {name} do usuário {userId} foi atualizado para {scheduledTime}");

            return true;
        }

        public Task<IEnumerable<Medicine>> GetMedicinesWithFewPills(long userId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            var now = DateTimeOffset.UtcNow;

            return _repositoryManager.MedicineRepository
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
                    .ContinueWith(task => task.Result.AsEnumerable());
        }

        public Task<IEnumerable<Medicine>> GetMedicinesToTakeTodayAsync(long userId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            var now = DateTimeOffset.UtcNow;

            return _repositoryManager.MedicineRepository
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
                   .ContinueWith(task => task.Result.AsEnumerable());
        }
    }
}

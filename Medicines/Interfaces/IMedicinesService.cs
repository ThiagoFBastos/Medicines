using Medicines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IMedicinesService
    {
        Task<IEnumerable<Medicine>> GetAllMedicinesAsync(long userId);
        Task<Medicine?> GetMedicineByIdAsync(Guid id);
        Task<Medicine?> GetMedicineByNameAsync(string name, long userId);
        Task<bool> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<bool> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<bool> AddMedicinePillsAsync(string name, int pillsQuantity, long userId);
        Task<bool> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId);
        Task<bool> DeleteMedicineAsync(string name, long userId);
        Task<IEnumerable<Medicine>> GetMedicinesWithFewPills(long userId);
        Task<IEnumerable<Medicine>> GetMedicinesToTakeTodayAsync(long userId);
    }
}

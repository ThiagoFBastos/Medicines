using Medicines.Models;
using Medicines.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IMedicinesService
    {
        Task<Result<IEnumerable<Medicine>, string>> GetAllMedicinesAsync(long userId);
        Task<Result<Medicine?, string>> GetMedicineByIdAsync(Guid id);
        Task<Result<Medicine?, string>> GetMedicineByNameAsync(string name, long userId);
        Task<Result<bool, string>> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, string>> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, string>> AddMedicinePillsAsync(string name, int pillsQuantity, long userId);
        Task<Result<bool, string>> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, string>> DeleteMedicineAsync(string name, long userId);
        Task<Result<IEnumerable<Medicine>, string>> GetMedicinesWithFewPills(long userId);
        Task<Result<IEnumerable<Medicine>, string>> GetMedicinesToTakeTodayAsync(long userId);
    }
}

using Medicines.Enums;
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
        Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetAllMedicinesAsync(long userId);
        Task<Result<Medicine?, EMedicinesStatusCode>> GetMedicineByIdAsync(Guid id);
        Task<Result<Medicine?, EMedicinesStatusCode>> GetMedicineByNameAsync(string name, long userId);
        Task<Result<bool, EMedicinesStatusCode>> AddMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, EMedicinesStatusCode>> UpdateMedicineAsync(string name, int pillsQuantity, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, EMedicinesStatusCode>> AddMedicinePillsAsync(string name, int pillsQuantity, long userId);
        Task<Result<bool, EMedicinesStatusCode>> UpdateMedicineScheduledTime(string name, DateTimeOffset scheduledTime, long userId);
        Task<Result<bool, EMedicinesStatusCode>> DeleteMedicineAsync(string name, long userId);
        Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetMedicinesWithFewPills(long userId);
        Task<Result<IEnumerable<Medicine>, EMedicinesStatusCode>> GetMedicinesToTakeTodayAsync(long userId);
    }
}

using Medicines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IMedicineRepository: IRepositoryBase<Medicine>
    {
        void AddMedicine(Medicine medicine);
        Task<Medicine?> GetMedicineByIdAsync(Guid id);
        Task<Medicine?> GetMedicineByNameAsync(string name, long userId);
        Task<IEnumerable<Medicine>> GetAllMedicinesAsync(long userId);
        void UpdateMedicine(Medicine medicine);
        void DeleteMedicine(Medicine medicine);
    }
}

using Medicines.Context;
using Medicines.Interfaces;
using Medicines.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Repository
{
    public class MedicineRepository: RepositoryBase<Medicine>, IMedicineRepository
    {
        public MedicineRepository(RepositoryContext context) : base(context)
        {

        }

        public void AddMedicine(Medicine medicine) => Add(medicine);

        public void DeleteMedicine(Medicine medicine) => Delete(medicine);

        public void UpdateMedicine(Medicine medicine) => Update(medicine);

        public Task<Medicine?> GetMedicineByIdAsync(Guid id)
            => FindByCondition(med => med.Id == id).FirstOrDefaultAsync();

        public Task<Medicine?> GetMedicineByNameAsync(string name, long userId)
            => FindByCondition(med => med.Name == name.ToLower() && med.UserId == userId)
               .FirstOrDefaultAsync();

        public Task<IEnumerable<Medicine>> GetAllMedicinesAsync(long userId)
            => FindByCondition(med => med.UserId == userId).ToListAsync()
               .ContinueWith(task => task.Result.AsEnumerable());
    }
}

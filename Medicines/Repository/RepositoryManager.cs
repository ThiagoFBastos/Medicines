using Medicines.Context;
using Medicines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Repository
{
    public class RepositoryManager: IRepositoryManager
    {
        public readonly Lazy<IMedicineRepository> _medicineRepository;
        public readonly RepositoryContext _context;

        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
            _medicineRepository = new Lazy<IMedicineRepository>(() => new MedicineRepository(context));
        }

        public IMedicineRepository MedicineRepository => _medicineRepository.Value;

        public Task SaveAsync() => _context.SaveChangesAsync();
    }
}

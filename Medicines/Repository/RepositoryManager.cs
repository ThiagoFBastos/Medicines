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
        private readonly Lazy<IMedicineRepository> _medicineRepository;
        private readonly Lazy<IUserRepository> _userRepository;
        private readonly RepositoryContext _context;

        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
            _medicineRepository = new Lazy<IMedicineRepository>(() => new MedicineRepository(context));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(context));
        }

        public IMedicineRepository MedicineRepository => _medicineRepository.Value;

        public IUserRepository UserRepository => _userRepository.Value;

        public Task SaveAsync() => _context.SaveChangesAsync();
    }
}

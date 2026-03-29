using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IRepositoryManager
    {
        IMedicineRepository MedicineRepository { get; }

        IUserRepository UserRepository { get; }

        Task SaveAsync();
    }
}

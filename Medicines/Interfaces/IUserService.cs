using Medicines.Models;
using Medicines.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IUserService
    {
        Task<Result<User?, string>> GetUserByUserIdAsync(long userId);

        Task<Result<bool, string>> AddUserAsync(long userId, string username);

        Task<Result<bool, string>> UpdateUserAsync(long userId, string username);

        Task<Result<bool, string>> DeleteUserAsync(long userId);

        Task<Result<IEnumerable<User>, string>> GetAllUsersAsync();
    }
}

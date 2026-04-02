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
    public interface IUserService
    {
        Task<Result<User?, EUserStatusCode>> GetUserByUserIdAsync(long userId);

        Task<Result<bool, EUserStatusCode>> AddUserAsync(long userId, string username);

        Task<Result<bool, EUserStatusCode>> UpdateUserAsync(long userId, string username);

        Task<Result<bool, EUserStatusCode>> DeleteUserAsync(long userId);

        Task<Result<IEnumerable<User>, EUserStatusCode>> GetAllUsersAsync();
    }
}

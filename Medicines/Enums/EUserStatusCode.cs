using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Enums
{
    public enum EUserStatusCode
    {
        USER_ALREADY_EXISTS,
        USER_NOT_FOUND,
        USER_DATA_INVALID,
        USER_ADD_ERROR,
        USER_UPDATE_ERROR,
        USER_DELETE_ERROR,
        USER_GET_ERROR,
        USER_LIST_ERROR,
        SUCCESS
    }
}

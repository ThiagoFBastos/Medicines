using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Enums
{
    public enum EMedicinesStatusCode
    {
        MEDICINE_ALREADY_EXISTS,
        MEDICINE_DATA_INVALID,
        SUCCESS,
        MEDICINE_ADD_ERROR,
        MEDICINE_UPDATE_ERROR,
        MEDICINE_DELETE_ERROR,
        MEDICINE_NOT_FOUND,
        MEDICINE_LIST_ERROR,
        MEDICINE_GET_ERROR
    }
}

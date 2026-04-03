using Medicines.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Utils
{
    public static class MedicinesStatusCodeTranslator
    {
        public static string TranslateStatusCode(EMedicinesStatusCode statusCode, string? name)
        {
            return statusCode switch
            {
                EMedicinesStatusCode.MEDICINE_ALREADY_EXISTS => $"O remédio {name} já existe.",
                EMedicinesStatusCode.MEDICINE_DATA_INVALID => $"Os dados do remédio {name} são inválidos.",
                EMedicinesStatusCode.SUCCESS => $"Operação realizada com sucesso para o remédio {name}.",
                EMedicinesStatusCode.MEDICINE_ADD_ERROR => $"Erro ao adicionar o remédio {name}.",
                EMedicinesStatusCode.MEDICINE_UPDATE_ERROR => $"Erro ao atualizar o remédio {name}.",
                EMedicinesStatusCode.MEDICINE_DELETE_ERROR => $"Erro ao deletar o remédio {name}.",
                EMedicinesStatusCode.MEDICINE_NOT_FOUND => $"Remédio {name} não encontrado.",
                EMedicinesStatusCode.MEDICINE_LIST_ERROR => $"Erro ao listar os remédios.",
                EMedicinesStatusCode.MEDICINE_GET_ERROR => $"Erro ao obter o remédio {name}.",
                _ => "Código de status desconhecido."
            };
        }
    }
}

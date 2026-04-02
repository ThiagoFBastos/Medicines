using Medicines.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Utils
{
    public static class UserStatusCodeTranslator
    {
        public static string TranslateUserStatusCode(EUserStatusCode statusCodeError)
        {
            return statusCodeError switch
            {
                EUserStatusCode.USER_ALREADY_EXISTS => "O usuário já existe",
                EUserStatusCode.USER_NOT_FOUND => "O usuário não foi encontrado",
                EUserStatusCode.USER_DATA_INVALID => "Os dados do usuário estão inválidos",
                EUserStatusCode.USER_ADD_ERROR => "Ocorreu um erro ao adicionar o usuário",
                EUserStatusCode.USER_UPDATE_ERROR => "Ocorreu um erro ao atualizar o usuário",
                EUserStatusCode.USER_DELETE_ERROR => "Ocorreu um erro ao deletar o usuário",
                EUserStatusCode.USER_GET_ERROR => "Ocorreu um erro ao obter o usuário",
                EUserStatusCode.USER_LIST_ERROR => "Ocorreu um erro ao listar os usuários",
                EUserStatusCode.SUCCESS => "Operação realizada com sucesso",
                _ => "Erro desconhecido"
            };
        }
    }
}

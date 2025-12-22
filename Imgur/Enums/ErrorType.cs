using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Enums
{
    public enum ErrorType
    {
        None,
        Validation,      // Dados inválidos
        Unauthorized,    // Precisa login
        Forbidden,       // Sem permissão
        NotFound,        // Recurso não existe
        Network,         // Erro de conexão
        Server,          // Erro 500
        Unknown          // Outros erros
    }
}

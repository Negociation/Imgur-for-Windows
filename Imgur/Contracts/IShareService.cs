using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Contracts
{
    public interface IShareService
    {

        void Initialize();
        void ShareMediaPost(string tituloPostagem, string conteudoPostagem);
    }
}

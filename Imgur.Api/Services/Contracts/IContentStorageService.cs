using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IContentStorageService
    {
        Task<string> SaveAsync(
            Uri contentUrl,
            string folderToken,
            string fileName,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

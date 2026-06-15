using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IFilePicker
    {
        Task<IReadOnlyList<SelectedFile>> PickMultipleFilesAsync();
    }
}

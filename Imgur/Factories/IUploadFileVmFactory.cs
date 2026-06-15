using Imgur.Enums;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.FileUpload;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface IUploadFileVmFactory
    {

        UploadFileViewModel GetUploadFileViewModel();
    }
}

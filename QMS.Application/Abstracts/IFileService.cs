using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface IFileService
    {
        Task<string> SaveUserImageAsync(IFormFile imageData);
    }
}

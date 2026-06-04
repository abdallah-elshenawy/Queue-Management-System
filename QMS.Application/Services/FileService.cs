using Microsoft.AspNetCore.Http;
using QMS.Application.Abstracts;

namespace QMS.Application.Services
{
    public class FileService : IFileService
    {
        public async Task<string> SaveUserImageAsync(IFormFile imageData)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileExtension = Path.GetExtension(imageData.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageData.CopyToAsync(stream);
            }

            return $"/images/users/{fileName}";
        }
    }
}

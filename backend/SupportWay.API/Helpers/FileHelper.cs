using Microsoft.AspNetCore.Http;

namespace SupportWay.API.Helpers
{
    public static class FileHelper
    {
        public static async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл не був переданий.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.Repositories
{
    public class DefaultAvatarRepository
    {
        private readonly SupportWayContext _context;

        public DefaultAvatarRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task AddDefaultAvatarAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Файл аватарки не знайдено.", imagePath);

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);

            var avatar = new DefaultAvatar
            {
                Id = Guid.NewGuid(),
                Image = imageBytes
            };

            _context.DefaultAvatars.Add(avatar);
            await _context.SaveChangesAsync();
        }

        public async Task<DefaultAvatar> GetFirstAsync()
        {
            return await _context.DefaultAvatars.FirstAsync();
        }
    }
}

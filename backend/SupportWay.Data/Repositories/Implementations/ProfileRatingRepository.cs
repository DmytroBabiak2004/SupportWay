using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;


namespace SupportWay.Data.Repositories.Implementations
{
    public class ProfileRatingRepository : IProfileRatingRepository
    {
        private readonly SupportWayContext _context;

        public ProfileRatingRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task RateProfileAsync(string raterUserId, Guid ratedProfileId, int value)
        {
            var existing = await _context.ProfileRatings
                .FirstOrDefaultAsync(x => x.RaterUserId == raterUserId && x.RatedProfileId == ratedProfileId);

            if (existing != null)
            {
                existing.Value = value;
                existing.RatedAt = DateTime.UtcNow;
            }
            else
            {
                var rating = new ProfileRating
                {
                    RaterUserId = raterUserId,
                    RatedProfileId = ratedProfileId,
                    Value = value,
                    RatedAt = DateTime.UtcNow
                };

                await _context.ProfileRatings.AddAsync(rating);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<double?> GetAverageRatingAsync(Guid ratedProfileId)
        {
            return await _context.ProfileRatings
                .Where(x => x.RatedProfileId == ratedProfileId)
                .AverageAsync(x => (double?)x.Value);
        }
    }

}

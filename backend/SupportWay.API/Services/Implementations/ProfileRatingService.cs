using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;

namespace SupportWay.Services.Implementations
{
    public class ProfileRatingService : IProfileRatingService
    {
        private readonly IProfileRatingRepository _ratingRepository;

        public ProfileRatingService(IProfileRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<double> RateProfileAsync(string raterUserId, Guid ratedProfileId, int value)
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Рейтинг має бути в межах від 1 до 5");

            await _ratingRepository.RateProfileAsync(raterUserId, ratedProfileId, value);

            var average = await _ratingRepository.GetAverageRatingAsync(ratedProfileId) ?? 0;

            return average;
        }

        public async Task<double> GetAverageRatingAsync(Guid ratedProfileId)
        {
            // Просто повертаємо середнє значення
            return await _ratingRepository.GetAverageRatingAsync(ratedProfileId) ?? 0;
        }
    }
}
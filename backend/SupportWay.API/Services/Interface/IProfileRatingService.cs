public interface IProfileRatingService
{
    Task<double> RateProfileAsync(string raterUserId, Guid ratedProfileId, int value);
    Task<double> GetAverageRatingAsync(Guid ratedProfileId);
}
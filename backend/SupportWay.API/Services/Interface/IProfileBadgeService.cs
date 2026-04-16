namespace SupportWay.API.Services.Interfaces
{
    public interface IProfileBadgeService
    {
        Task RemoveBadgeFromProfileAsync(Guid profileId, Guid badgeId);
        Task AwardBadgeToProfileAsync(Guid profileId, Guid badgeId);
    }
}
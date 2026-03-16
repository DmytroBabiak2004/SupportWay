namespace SupportWay.API.Services.Interfaces
{
    public interface IProfileAnalyticsService
    {
        Task<object> GetDashboardAsync(Guid profileId);
    }
}
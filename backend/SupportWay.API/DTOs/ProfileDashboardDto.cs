namespace SupportWay.API.DTOs
{
    public class ProfileDashboardDto
    {
        public int TotalPosts { get; set; }
        public int TotalRequests { get; set; }
        public int ClosedRequests { get; set; }

        public List<PostsByDateDto> PostsByDate { get; set; } = new();
        public List<ActivityHeatmapItemDto> ActivityHeatmap { get; set; } = new();
        public List<TypeCountDto> RequestsByType { get; set; } = new();
        public List<TypeCountDto> ClosedRequestsByType { get; set; } = new();
    }
}
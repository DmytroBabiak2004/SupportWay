namespace SupportWay.API.DTOs
{
    public class UserSearchDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string? Name { get; set; }
        public string? FullName { get; set; }
        public byte[]? PhotoBase64 { get; set; }
    }
}
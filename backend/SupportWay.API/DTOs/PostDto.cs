public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public byte[]? Image { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = null!;

    public string? AuthorUserName { get; set; }
    public string? AuthorFullName { get; set; }
    public byte[]? AuthorPhotoBase64 { get; set; }

    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    public bool IsLikedByCurrentUser { get; set; } = false;
}

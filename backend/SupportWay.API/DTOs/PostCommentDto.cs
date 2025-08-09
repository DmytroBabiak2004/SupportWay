public class PostCommentDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; } 
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePostCommentDto
{
    public Guid PostId { get; set; }
    public Guid RequestId { get; set; }
    public string Text { get; set; }
}

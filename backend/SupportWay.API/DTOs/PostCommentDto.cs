public class PostCommentDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; } 
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePostCommentDto
{
    public int? PostId { get; set; }
    public int? RequestId { get; set; }
    public string Text { get; set; }
}

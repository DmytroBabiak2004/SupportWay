using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SupportWay.Data.Models;

namespace SupportWay.Data.Mongo.Documents;

public class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;

    public Guid? SharedPostId { get; set; }

    public Guid? SharedHelpRequestId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? EditedAt { get; set; }

    public List<string> ReadByUserIds { get; set; } = new();
}
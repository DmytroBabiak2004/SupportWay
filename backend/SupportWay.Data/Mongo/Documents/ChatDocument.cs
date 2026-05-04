using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SupportWay.Data.Mongo.Documents;

public class ChatDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public bool IsPrivate { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public List<ChatParticipantDocument> Participants { get; set; } = new();

    public Guid? LastMessageId { get; set; }

    public DateTime? LastMessageAt { get; set; }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SupportWay.Data.Mongo.Documents;

public class ChatParticipantDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastReadAt { get; set; }
}
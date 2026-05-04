using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SupportWay.Data.Mongo.Documents;

namespace SupportWay.Data.Mongo;

public class MongoChatContext
{
    private readonly IMongoDatabase _database;

    public MongoChatContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<ChatDocument> Chats =>
        _database.GetCollection<ChatDocument>("chats");

    public IMongoCollection<MessageDocument> Messages =>
        _database.GetCollection<MessageDocument>("messages");

    public IMongoCollection<ChatParticipantDocument> ChatParticipants =>
        _database.GetCollection<ChatParticipantDocument>("chat_participants");
}
namespace SupportWay.Data.Mongo;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "SupportWayChatDb";
    public string ChatsCollectionName { get; set; } = "chats";
}

using MessageManager.Models;
using MongoDB.Driver;
using static MessageManager.Models.MessageStatus.MessageStatusEnum;

namespace MessageManager.Services;

public class MongoService {
  public MongoService(IConfiguration config) {
    var client = new MongoClient(config["MongoSettings:ConnectionString"]);
    Database = client.GetDatabase(config["MongoSettings:Database"]);
    Messages = Database.GetCollection<Message>("messages");
    Subscribers = Database.GetCollection<Subscriber>("subscribers");
  }

  public IMongoCollection<Message> Messages { get; }
  public IMongoCollection<Subscriber> Subscribers { get; }

  public IMongoCollection<Message> DeadMessages =>
    Database.GetCollection<Message>("DeadMessages");

  public IMongoDatabase Database { get; }

  public async Task<List<Message>> GetMessagesToProcess() {
    var filterByLastAttempt = Builders<Message>.Filter.Where(message => message.LastAttempt < DateTime.UtcNow);
    var filterByStatus = Builders<Message>.Filter.Where(message => message.Status.Id == (int)Pending ||
                                                                   message.Status.Id == (int)Retrying);
    var filter = Builders<Message>.Filter.And(filterByLastAttempt, filterByStatus);
    var messages = await Messages.Find(filter).SortBy(message => message.Priority).Limit(100).ToListAsync() ?? [];
    return messages;
  }

  public async Task UpdateMessage(Message message) {
    var filter = Builders<Message>.Filter.Eq(m => m.Id, message.Id);
    var update = Builders<Message>.Update
      .Set(m => m.Status, message.Status)
      .Set(m => m.LastAttempt, message.LastAttempt)
      .Set(m => m.RetryCount, message.RetryCount);
    await Messages.UpdateOneAsync(filter, update);
  }

  public async Task<List<Subscriber>> GetAllSubscribers() {
    return await Subscribers.Find(_ => true).ToListAsync() ?? [];
  }
}
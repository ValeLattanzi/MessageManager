using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessageManager.Models;

public class Subscriber {
  private Subscriber() {
    QueueName = string.Empty;
    SubscriberUrl = string.Empty;
    Method = HttpMethod.Post;
  }

  public Subscriber(string queueName, string subscriberUrl, HttpMethod httpMethod) : this() {
    QueueName = queueName;
    SubscriberUrl = subscriberUrl;
    Method = httpMethod;
  }

  [BsonId] public ObjectId Id { get; set; }

  public string QueueName { get; private set; }
  public string SubscriberUrl { get; private set; }
  public HttpMethod Method { get; set; }
}
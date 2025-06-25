using System.Text.Json;
using MongoDB.Bson;

namespace MessageManager.Models;

/// <summary>
///   This class manage to publish messages
/// </summary>
/// <param name="broker"></param>
public class Producer(MessageBroker broker) {
  public void Send<T>(string eventType, T eventData, int retries) {
    var jsonBody = JsonSerializer.Serialize(eventData);
    var bsonDocument = BsonDocument.Parse(jsonBody);
    _ = broker.PublishAsync(eventType, new(eventType, bsonDocument, retries));
  }
}
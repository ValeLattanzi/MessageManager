using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessageManager.Models;

public class Message {
  public enum PriorityEnum {
    High = 3,
    Medium = 2,
    Low = 1
  }

  public Message() {
    Status = new(MessageStatus.MessageStatusEnum.Pending);
    QueueName = string.Empty;
    MaxRetries = 5;
    RetryCount = 0;
    Content = new();
  }

  public Message(string queueName, BsonDocument content, int maxRetries) : this() {
    QueueName = queueName;
    MaxRetries = maxRetries;
    CreatedAt = DateTime.Now;
    Content = content;
  }

  [BsonId, Key] public ObjectId Id { get; set; }

  public string QueueName { get; private set; }
  public BsonDocument Content { get; private set; }
  public int RetryCount { get; private set; }
  public int MaxRetries { get; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? LastAttempt { get; private set; }
  public DateTime? NextRetryAt { get; set; }
  public MessageStatus Status { get; private set; }
  public DateTime? DeliverAt { get; set; }
  public int Priority { get; set; }
  public DateTime? ProcessingSince { get; set; }

  public void UpdateStatus(MessageStatus newStatus) {
    Status = newStatus;
  }

  public void CountRetry() {
    RetryCount += 1;
    LastAttempt = DateTime.UtcNow;
    if (RetryCount >= MaxRetries)
      UpdateStatus(new(MessageStatus.MessageStatusEnum.Failed));
    else {
      NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, RetryCount)); // Exponential backoff
      UpdateStatus(new(MessageStatus.MessageStatusEnum.Retrying));
    }
  }
}
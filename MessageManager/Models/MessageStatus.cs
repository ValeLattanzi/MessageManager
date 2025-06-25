using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace MessageManager.Models;

public class MessageStatus {
  public enum MessageStatusEnum {
    Pending = 1,
    Approved,
    Canceled,
    Failed,
    Retrying,
    Processing
  }

  public MessageStatus() { }

  public MessageStatus(MessageStatusEnum messageStatus) {
    Id = (int)messageStatus;
    Name = messageStatus.ToString();
  }

  /// <summary>
  ///   Constructor required for EF
  /// </summary>
  /// <param name="name"></param>
  public MessageStatus(string name) {
    Name = name;
  }

  [Key, BsonId] public int Id { get; set; }

  [MaxLength(20)] public string Name { get; set; } = string.Empty;

  public bool IsPending() {
    return Id == (int)MessageStatusEnum.Pending;
  }

  public bool IsApproved() {
    return Id == (int)MessageStatusEnum.Approved;
  }

  public bool IsCancelled() {
    return Id == (int)MessageStatusEnum.Canceled;
  }

  public bool IsFailed() {
    return Id == (int)MessageStatusEnum.Failed;
  }

  public bool IsRetrying() {
    return Id == (int)MessageStatusEnum.Retrying;
  }

  public bool IsProcessing() {
    return Id == (int)MessageStatusEnum.Processing;
  }
}
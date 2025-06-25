namespace MessageManager.Models;

/// <summary>
///   Message queue to process
/// </summary>
public class MessageQueue {
  private readonly Queue<Message> _queue = new();

  public void Enqueue(Message message) {
    _queue.Enqueue(message);
  }

  public bool TryDequeue(out Message? message) {
    return _queue.TryDequeue(out message);
  }
}
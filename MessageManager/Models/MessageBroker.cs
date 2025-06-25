namespace MessageManager.Models;

public class MessageBroker {
  private readonly Dictionary<string, MessageQueue> _queues = new();
  private readonly Dictionary<string, List<Action<Message>>> _subscribers = new();

  public async Task PublishAsync(string topic, Message message) {
    if (!_queues.ContainsKey(topic)) _queues[topic] = new();

    _queues[topic].Enqueue(message);
    await NotifySubscribersAsync(topic, message);
  }

  public void Subscribe(string topic, Action<Message> callback) {
    if (!_subscribers.ContainsKey(topic)) _subscribers[topic] = new();

    _subscribers[topic].Add(callback);
  }

  private async Task NotifySubscribersAsync(string topic, Message message) {
    if (_subscribers.TryGetValue(topic, out var subscribers)) {
      var tasks = subscribers.Select(subscriber => Task.Run(() => subscriber(message)));
      await Task.WhenAll(tasks);
    }
  }
}
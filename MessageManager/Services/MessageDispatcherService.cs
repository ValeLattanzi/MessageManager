using System.Text;
using MessageManager.Models;
using MongoDB.Bson;
using Serilog;

namespace MessageManager.Services;

public class MessageDispatcherService(MongoService mongoService, IHttpClientFactory httpClientFactory)
  : BackgroundService {
  private readonly HttpClient _httpClient = httpClientFactory.CreateClient("InsecureClient");

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    try {
      while (!stoppingToken.IsCancellationRequested) {
        var messagesToProcess = await mongoService.GetMessagesToProcess();
        if (messagesToProcess.Count == 0) await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

        Log.Information("Processing {Count} messages.", messagesToProcess.Count);

        // Recuperar todos los suscriptores y agruparlos por QueueName
        var allSubscribers = await mongoService.GetAllSubscribers(); // Debes implementar este método en MongoService
        var subscribersByQueue = allSubscribers
          .GroupBy(s => s.QueueName)
          .ToDictionary(g => g.Key, g => g.ToList());

        var parallelOptions = new ParallelOptions {
          MaxDegreeOfParallelism = 25,
          CancellationToken = stoppingToken
        };

        try {
          await Parallel.ForEachAsync(messagesToProcess, parallelOptions,
            async (message, token) => { await ProcessMessage(message, subscribersByQueue, token); });
        }
        catch (Exception ex) {
          Log.Error(ex, "Error processing messages.");
        }
      }
    }
    catch (OperationCanceledException) {
      Log.Error("Message Dispatcher was ended.");
    }
  }

  private async Task ProcessMessage(Message message,
    Dictionary<string, List<Subscriber>> subscribersByQueue, CancellationToken cancellationToken) {
    if (!subscribersByQueue.TryGetValue(message.QueueName, out var subscribers) || subscribers.Count == 0) {
      Log.Information("No subscribers found for message {MessageId} in queue {QueueName}.", message.Id,
        message.QueueName);
      return;
    }

    // Check if the message is ready to be processed
    if (DateTime.Now < message.NextRetryAt) return;

    try {
      foreach (var subscriber in subscribers) {
        Log.Information("Processing message {MessageId} for subscriber {SubscriberId} at {SubscriberUrl}.",
          message.Id, subscriber.Id, subscriber.SubscriberUrl);
        if (cancellationToken.IsCancellationRequested) {
          Log.Warning("Cancellation requested, stopping processing for message {MessageId}.", message.Id);
          break;
        }

        var messageContent = message.Content.ToJson();
        try {
          var response =
            await _httpClient.SendAsync(new(subscriber.Method, subscriber.SubscriberUrl) {
              Content = new StringContent(messageContent, Encoding.UTF8, "application/json")
            }, cancellationToken);
          response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) {
          Log.Error(ex, "HTTP request failed for message {MessageId} to subscriber {SubscriberId}.",
            message.Id, subscriber.Id);
          message.CountRetry();
        }
        catch (Exception ex) {
          Log.Error(ex, "Error processing message {MessageId} for subscriber {SubscriberId}.",
            message.Id, subscriber.Id);
          message.CountRetry();
        }
        finally {
          // Persistir el mensaje después de cada intento fallido
          try {
            await mongoService.UpdateMessage(message);
          }
          catch (Exception updateEx) {
            Log.Error(updateEx, "Error updating message {MessageId} after retry.", message.Id);
          }
        }
      }
    }
    catch (Exception ex) {
      Log.Error(ex, "Error processing message {MessageId}", message.Id);
    }
  }
}
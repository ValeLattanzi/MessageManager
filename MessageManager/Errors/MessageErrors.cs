using result_pattern;

namespace MessageManager.Errors;

public static class MessageErrors;

public static class SubscriberErrors {
  public static Error SubscriberAlreadyExists(string topic, string subscriberUrl) {
    return new("Subscribers.AlreadyExists", $"Topic '{topic}' already exists, subscriber '{subscriberUrl}'.",
      ErrorType.BadRequest);
  }
}
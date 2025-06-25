namespace MessageManager.Requests;

public record SubscribeRequest(string QueueName, string SubscriberUrl, HttpMethod Method);
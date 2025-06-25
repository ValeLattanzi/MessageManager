using System.ComponentModel.DataAnnotations;
using MessageManager.Errors;
using MessageManager.Models;
using MessageManager.Requests;
using MessageManager.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using result_pattern;

namespace MessageManager.Controllers;

[ApiController, Route("api/[controller]")]
public class MessageController(MongoService mongoService) : ControllerBase {
  /// <summary>
  ///   Endpoint to publish messages
  /// </summary>
  /// <param name="queueName"></param>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  [HttpPost("publish")]
  public async Task<IActionResult> Publish([FromQuery, Required] string queueName,
    [FromBody] SendMessageRequest request, CancellationToken cancellationToken = default) {
    var contentBson = BsonSerializer.Deserialize<BsonDocument>(request.Content.GetRawText());
    var message = new Message(queueName, contentBson, request.MaxRetries);

    await mongoService.Messages.InsertOneAsync(message, cancellationToken: cancellationToken);
    return Ok(new {
      Topic = queueName.Normalize(),
      Message = "Message published."
    });
  }

  [HttpPost("subscribe")]
  public async Task<IActionResult> Subscribe([FromBody, Required] SubscribeRequest request) {
    var filterBuilder = Builders<Subscriber>.Filter;
    var filter = Builders<Subscriber>.Filter.And(filterBuilder.Eq(s => s.QueueName, request.QueueName),
      filterBuilder.Eq(s => s.SubscriberUrl, request.SubscriberUrl));
    var cursor = await mongoService.Subscribers.FindAsync(filter);
    var alreadyExists = await cursor.AnyAsync();
    if (alreadyExists) {
      return BadRequest(Result
        .failure(SubscriberErrors.SubscriberAlreadyExists(request.QueueName, request.SubscriberUrl))
        .toProblemDetails());
    }

    var subscriber = new Subscriber(request.QueueName, request.SubscriberUrl, request.Method);
    await mongoService.Subscribers.InsertOneAsync(subscriber);
    return Ok(new {
      Topic = request.QueueName.Normalize(),
      Message = "Subscriber registered."
    });
  }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MessageManager.Requests;

public record SendMessageRequest(
  [Required] JsonElement Content,
  [Required, Range(1, 10)] int MaxRetries);
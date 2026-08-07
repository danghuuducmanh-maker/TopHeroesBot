using System.Text.Json.Serialization;

namespace TopHeroesBot.Application.DTOs;

public class GiftCodeResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
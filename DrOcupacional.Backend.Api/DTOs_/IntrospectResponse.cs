using System.Text.Json.Serialization;

namespace DrOcupacional.Backend.Api.DTOs;

/// <summary>
/// DTO para resposta do endpoint de introspect do Identity Manager
/// </summary>
public class IntrospectResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
    
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    [JsonPropertyName("exp")]
    public long? Exp { get; set; }
    
    [JsonPropertyName("iat")]
    public long? Iat { get; set; }
    
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }
    
    [JsonPropertyName("aud")]
    public string? Aud { get; set; }
    
    [JsonPropertyName("iss")]
    public string? Iss { get; set; }
}


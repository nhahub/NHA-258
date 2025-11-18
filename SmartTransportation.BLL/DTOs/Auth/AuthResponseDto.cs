using System.Text.Json.Serialization;

public class AuthResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;


    [JsonPropertyName("userTypeId")]
    public int UserTypeId { get; set; }

}

using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Users;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = "User"; // Admin, Inspector, User
    
    [JsonPropertyName("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }
    
    [JsonPropertyName("isStudent")]
    public bool IsStudent { get; set; } = false;
    
    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}


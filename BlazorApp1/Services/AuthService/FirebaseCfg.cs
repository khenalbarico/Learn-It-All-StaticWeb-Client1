using System.Text.Json.Serialization;

namespace BlazorApp1.Services.AuthService;

// Bound from wwwroot/appsettings*.json. These are Firebase *web* config values, which
// are public by design — the API still verifies every token with Firebase Admin.
public class FirebaseCfg
{
    [JsonPropertyName("apiKey")]            public string ApiKey            { get; set; } = "";
    [JsonPropertyName("authDomain")]        public string AuthDomain        { get; set; } = "";
    [JsonPropertyName("projectId")]         public string ProjectId         { get; set; } = "";
    [JsonPropertyName("storageBucket")]     public string StorageBucket     { get; set; } = "";
    [JsonPropertyName("messagingSenderId")] public string MessagingSenderId { get; set; } = "";
    [JsonPropertyName("appId")]             public string AppId             { get; set; } = "";
}

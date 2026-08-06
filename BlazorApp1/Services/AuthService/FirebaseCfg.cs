using System.Text.Json.Serialization;

namespace BlazorApp1.Services.AuthService;

// Only the three fields Firebase Authentication actually needs.
//
// These are identifiers, not secrets — the SDK ships them in every request from the
// browser, so there is nothing to leak by having them here. What actually protects the
// project is server-side: the API verifies every ID token with Firebase Admin, and
// Firebase only issues tokens to origins listed under Authentication -> Authorized domains.
//
// storageBucket, messagingSenderId, appId, databaseURL and measurementId are deliberately
// omitted — they address Cloud Storage, FCM, Realtime Database and Analytics, none of
// which this app uses. Publishing config for services you don't use only widens the
// surface someone can probe.
public class FirebaseCfg
{
    [JsonPropertyName("apiKey")]     public string ApiKey     { get; set; } = "";
    [JsonPropertyName("authDomain")] public string AuthDomain { get; set; } = "";
    [JsonPropertyName("projectId")]  public string ProjectId  { get; set; } = "";
}

using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class UserInfo
{
    [Required] public string Uid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserSubscription Subscription { get; set; } = UserSubscription.Free;
    public List<string> Keywords { get; set; } = [];
    public List<UserLibrary> Library { get; set; } = [];
}

namespace BlazorApp1.Models;

public class UserInfo
{
    public string            Uid          { get; set; } = string.Empty;
    public string?           Email        { get; set; }
    public string?           FirstName    { get; set; }
    public string?           LastName     { get; set; }
    public string?           PhoneNumber  { get; set; }
    public UserSubscription  Subscription { get; set; } = UserSubscription.Free;
    public List<UserLibrary> Library      { get; set; } = [];

    public string DisplayName
        => string.Join(' ', new[] { FirstName, LastName }.Where(n => !string.IsNullOrWhiteSpace(n)))
               is { Length: > 0 } name
           ? name
           : Email ?? "Reader";
}

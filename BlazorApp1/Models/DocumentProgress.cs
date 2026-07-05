namespace BlazorApp1.Models;

public class DocumentProgress
{
    public string   DocUid     { get; set; } = "";
    public int      Page       { get; set; }
    public int      TotalPages { get; set; }
    public DateTime LastReadAt { get; set; }
}

namespace NordicHiking.Core.Models;

public class Channel
{
    public int Id { get; set; }
    public string YoutubeChannelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public ICollection<Video> Videos { get; set; } = new List<Video>();
}

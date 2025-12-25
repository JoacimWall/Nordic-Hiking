namespace NordicHiking.Core.Models;

public class Video
{
    public int Id { get; set; }
    public string YoutubeVideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Transcript { get; set; }
    public string? VideoTalkSummary { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsProcessed { get; set; }
    public bool ShowOnMap { get; set; }
    public int ChannelId { get; set; }
    public Channel? Channel { get; set; }
    public ICollection<HikeLocation> Locations { get; set; } = new List<HikeLocation>();
}

namespace NordicHiking.Core.Models;

public class HikeLocation
{
    public int Id { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public int VideoId { get; set; }
    public Video? Video { get; set; }
}

using NordicHiking.Core.Models;

namespace NordicHiking.Core.Interfaces;

public interface IAiAnalysisService
{
    Task<HikeAnalysisResult> AnalyzeVideoAsync(string title, string description, string? transcript);
    Task<string> SummarizeTranscriptAsync(string transcript);
}

public class HikeAnalysisResult
{
    public List<PlaceInfo> Places { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string Confidence { get; set; } = string.Empty;
}

public class PlaceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

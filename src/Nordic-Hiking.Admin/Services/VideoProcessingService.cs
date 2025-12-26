using Microsoft.EntityFrameworkCore;
using NordicHiking.Core.Interfaces;
using NordicHiking.Core.Models;
using NordicHiking.Data;

namespace NordicHiking.Admin.Services;

public class VideoProcessingService
{
    private readonly AppDbContext _dbContext;
    private readonly IYouTubeService _youTubeService;
    private readonly IAiAnalysisService _aiService;
    private readonly IGeocodingService _geocodingService;

    public VideoProcessingService(
        AppDbContext dbContext,
        IYouTubeService youTubeService,
        IAiAnalysisService aiService,
        IGeocodingService geocodingService)
    {
        _dbContext = dbContext;
        _youTubeService = youTubeService;
        _aiService = aiService;
        _geocodingService = geocodingService;
    }

    public event Action<string>? OnStatusChanged;

    private void UpdateStatus(string status)
    {
        OnStatusChanged?.Invoke(status);
    }

    public async Task<List<HikeLocation>> ProcessVideoAsync(Video video)
    {
        var locations = new List<HikeLocation>();

        // Säkerställ att video är tracked av denna DbContext
        var trackedVideo = await _dbContext.Videos.FindAsync(video.Id);
        if (trackedVideo == null)
        {
            throw new InvalidOperationException($"Video med ID {video.Id} hittades inte.");
        }

        // Hämta transkript
        UpdateStatus("Hämtar transkript...");
        trackedVideo.Transcript = await _youTubeService.GetVideoTranscriptAsync(trackedVideo.YoutubeVideoId);

        // Sammanfatta transkriptet
        if (!string.IsNullOrEmpty(trackedVideo.Transcript))
        {
            UpdateStatus("Sammanfattar transkript...");
            trackedVideo.VideoTalkSummary = await _aiService.SummarizeTranscriptAsync(trackedVideo.Transcript);
        }

        // AI-analys
        UpdateStatus("Analyserar med AI...");
        var analysis = await _aiService.AnalyzeVideoAsync(trackedVideo.Title, trackedVideo.Description, trackedVideo.Transcript);

        // Ta bort gamla platser
        // var oldLocations = await _dbContext.HikeLocations.Where(l => l.VideoId == trackedVideo.Id).ToListAsync();
        // _dbContext.HikeLocations.RemoveRange(oldLocations);

        // Geokodning och lägg till nya platser
        UpdateStatus("Hämtar koordinater...");

        foreach (var place in analysis.Places)
        {
            var coords = await _geocodingService.GetCoordinatesAsync(place.Name, place.Region, place.Country);

            if (coords.HasValue)
            {
                var location = new HikeLocation
                {
                    PlaceName = place.Name ?? string.Empty,
                    Region = place.Region ?? string.Empty,
                    Country = place.Country ?? string.Empty,
                    Latitude = coords.Value.Latitude,
                    Longitude = coords.Value.Longitude,
                    Summary = analysis.Summary ?? string.Empty,
                    Difficulty = analysis.Difficulty ?? string.Empty,
                    Duration = analysis.Duration,
                    Confidence = analysis.Confidence ?? string.Empty,
                    VideoId = trackedVideo.Id
                };
           
                _dbContext.HikeLocations.Add(location);
                locations.Add(location);
                break;
            }
        }

        trackedVideo.IsProcessed = true;
        trackedVideo.ShowOnMap = true;
        await _dbContext.SaveChangesAsync();

        // Uppdatera original-objektet med nya värden
        video.Transcript = trackedVideo.Transcript;
        video.VideoTalkSummary = trackedVideo.VideoTalkSummary;
        video.IsProcessed = trackedVideo.IsProcessed;
        video.ShowOnMap = trackedVideo.ShowOnMap;

        UpdateStatus("Video processad!");

        return locations;
    }
}


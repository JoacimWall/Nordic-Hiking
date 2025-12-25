using Microsoft.JSInterop;
using System.Text.Json;

namespace NordicHiking.Public.Services;

public class HikeDataService
{
    private readonly IJSRuntime _jsRuntime;
    private List<HikeLocationDto>? _cachedLocations;
    private bool _isInitialized = false;

    public HikeDataService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<HikeLocationDto>> GetAllLocationsAsync()
    {
        if (_cachedLocations != null)
            return _cachedLocations;

        var locations = new List<HikeLocationDto>();

        try
        {
            // Initialize the database if not already done
            if (!_isInitialized)
            {
                var initialized = await _jsRuntime.InvokeAsync<bool>("sqliteHelper.initialize", "data/hikes.db");
                if (!initialized)
                {
                    Console.WriteLine("Failed to initialize database");
                    return locations;
                }
                _isInitialized = true;
            }

            // Query locations from database
            var result = await _jsRuntime.InvokeAsync<JsonElement>("sqliteHelper.queryLocations");

            // Deserialize the JSON result
            if (result.ValueKind == JsonValueKind.Array)
            {
                locations = JsonSerializer.Deserialize<List<HikeLocationDto>>(result.GetRawText()) ?? new List<HikeLocationDto>();
            }

            _cachedLocations = locations;
            Console.WriteLine($"Loaded {locations.Count} locations from database");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading hike data: {ex.Message}");
        }

        return _cachedLocations ?? new List<HikeLocationDto>();
    }
}

public class HikeLocationDto
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
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoThumbnail { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public string? VideoTalkSummary { get; set; }
}

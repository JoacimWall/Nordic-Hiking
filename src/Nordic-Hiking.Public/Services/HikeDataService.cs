using Microsoft.JSInterop;
using System.Text.Json;

namespace NordicHiking.Public.Services;

public class HikeDataService
{
    private readonly IJSRuntime _jsRuntime;
    private List<HikeLocationDto>? _cachedLocations;
    private List<ChannelDto>? _cachedChannels;
    private bool _isInitialized = false;

    public HikeDataService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            var initialized = await _jsRuntime.InvokeAsync<bool>("sqliteHelper.initialize", "data/hikes.db");
            if (!initialized)
            {
                throw new Exception("Failed to initialize database");
            }
            _isInitialized = true;
        }
    }

    public async Task<List<HikeLocationDto>> GetAllLocationsAsync()
    {
        if (_cachedLocations != null)
            return _cachedLocations;

        var locations = new List<HikeLocationDto>();

        try
        {
            await EnsureInitializedAsync();

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

    public async Task<List<ChannelDto>> GetAllChannelsAsync()
    {
        if (_cachedChannels != null)
            return _cachedChannels;

        var channels = new List<ChannelDto>();

        try
        {
            await EnsureInitializedAsync();

            // Query channels from database
            var result = await _jsRuntime.InvokeAsync<JsonElement>("sqliteHelper.queryChannels");

            // Deserialize the JSON result
            if (result.ValueKind == JsonValueKind.Array)
            {
                channels = JsonSerializer.Deserialize<List<ChannelDto>>(result.GetRawText()) ?? new List<ChannelDto>();
            }

            _cachedChannels = channels;
            Console.WriteLine($"Loaded {channels.Count} channels from database");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading channel data: {ex.Message}");
        }

        return _cachedChannels ?? new List<ChannelDto>();
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
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public string? VideoTalkSummary { get; set; }
}

public class ChannelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


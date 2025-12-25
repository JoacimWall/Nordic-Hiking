using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using NordicHiking.Core.Interfaces;

namespace NordicHiking.Admin.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _userAgent;
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(1);

    public GeocodingService(IConfiguration configuration)
    {
        _userAgent = configuration["Nominatim:UserAgent"]
            ?? throw new InvalidOperationException("Nominatim UserAgent not configured");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
    }

    public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string placeName, string region, string country)
    {
        // Respektera rate limiting (1 request/sekund)
        var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
        if (timeSinceLastRequest < _minRequestInterval)
        {
            await Task.Delay(_minRequestInterval - timeSinceLastRequest);
        }

        _lastRequestTime = DateTime.UtcNow;

        try
        {
            // Bygg sökquery
            var query = string.IsNullOrEmpty(region)
                ? $"{placeName}, {country}"
                : $"{placeName}, {region}, {country}";

            var response = await _httpClient.GetAsync($"search?q={Uri.EscapeDataString(query)}&format=json&limit=1");

            if (!response.IsSuccessStatusCode)
                return null;

            var results = await response.Content.ReadFromJsonAsync<List<NominatimResult>>();

            if (results == null || results.Count == 0)
                return null;

            var result = results[0];
            return (double.Parse(result.Lat, CultureInfo.InvariantCulture),
                    double.Parse(result.Lon, CultureInfo.InvariantCulture));
        }
        catch
        {
            return null;
        }
    }

    private class NominatimResult
    {
        public string Lat { get; set; } = string.Empty;
        public string Lon { get; set; } = string.Empty;
    }
}

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NordicHiking.Core.Interfaces;

namespace NordicHiking.Admin.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _nominatimClient;
    private readonly HttpClient _googleClient;
    private readonly string _userAgent;
    private readonly string? _googleApiKey;
    private DateTime _lastNominatimRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(1);

    public GeocodingService(IConfiguration configuration)
    {
        _userAgent = configuration["Nominatim:UserAgent"]
            ?? throw new InvalidOperationException("Nominatim UserAgent not configured");

        _googleApiKey = configuration["Google:MapsApiKey"];

        _nominatimClient = new HttpClient
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        };
        _nominatimClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);

        _googleClient = new HttpClient
        {
            //BaseAddress = new Uri("https://maps.googleapis.com/")
            BaseAddress = new Uri("https://places.googleapis.com/")
        };
    }

    public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string placeName, string region, string country)
    {
        // Försök med Google Maps först om API-nyckel finns
        if (!string.IsNullOrEmpty(_googleApiKey))
        {
            var googleResult = await GetCoordinatesFromGoogleAsync(placeName, region, country);
            if (googleResult.HasValue)
                return googleResult;
        }

        // Fallback till Nominatim
        return await GetCoordinatesFromNominatimAsync(placeName, region, country);
    }

    public async Task<(double Latitude, double Longitude)?> GetCoordinatesFromGoogleAsync(string placeName, string region, string country)
    {
        if (string.IsNullOrEmpty(_googleApiKey))
            return null;

        try
        {
            var query = string.IsNullOrEmpty(region)
                ? $"{placeName}, {country}"
                : $"{placeName}, {region}, {country}";

            var url = $"maps/api/geocode/json?address={Uri.EscapeDataString(query)}&key={_googleApiKey}";
            var response = await _googleClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = await response.Content.ReadFromJsonAsync<GoogleGeocodingResponse>(options);

            if (result == null)
                return null;

            // Hantera fel från Google API
            if (result.Status == "REQUEST_DENIED")
            {
                Console.WriteLine($"Google Geocoding API: REQUEST_DENIED - {result.ErrorMessage}");
                Console.WriteLine("Se till att Geocoding API är aktiverat i Google Cloud Console: https://console.cloud.google.com/apis/library/geocoding-backend.googleapis.com");
                return null;
            }

            if (result.Status != "OK" || result.Results == null || result.Results.Count == 0)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    Console.WriteLine($"Google Geocoding API error: {result.Status} - {result.ErrorMessage}");
                return null;
            }

            var location = result.Results[0].Geometry?.Location;
            if (location == null)
                return null;

            return (location.Lat, location.Lng);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google Geocoding exception: {ex.Message}");
            return null;
        }
    }

    private async Task<(double Latitude, double Longitude)?> GetCoordinatesFromNominatimAsync(string placeName, string region, string country)
    {
        // Respektera rate limiting (1 request/sekund)
        var timeSinceLastRequest = DateTime.UtcNow - _lastNominatimRequestTime;
        if (timeSinceLastRequest < _minRequestInterval)
        {
            await Task.Delay(_minRequestInterval - timeSinceLastRequest);
        }

        _lastNominatimRequestTime = DateTime.UtcNow;

        try
        {
            var query = string.IsNullOrEmpty(region)
                ? $"{placeName}, {country}"
                : $"{placeName}, {region}, {country}";

            var response = await _nominatimClient.GetAsync($"search?q={Uri.EscapeDataString(query)}&format=json&limit=1");

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

    private class GoogleGeocodingResponse
    {
        public string Status { get; set; } = string.Empty;
        public List<GoogleGeocodingResult>? Results { get; set; }
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }

    private class GoogleGeocodingResult
    {
        public GoogleGeometry? Geometry { get; set; }
    }

    private class GoogleGeometry
    {
        public GoogleLocation? Location { get; set; }
    }

    private class GoogleLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}

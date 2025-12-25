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

            var requestBody = new
            {
                textQuery = query
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "v1/places:searchText")
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("X-Goog-Api-Key", _googleApiKey);
            request.Headers.Add("X-Goog-FieldMask", "places.location");

            var response = await _googleClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Google Places API error: {response.StatusCode} - {errorContent}");
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = await response.Content.ReadFromJsonAsync<PlacesTextSearchResponse>(options);

            if (result?.Places == null || result.Places.Count == 0)
                return null;

            var location = result.Places[0].Location;
            if (location == null)
                return null;

            return (location.Latitude, location.Longitude);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google Places API exception: {ex.Message}");
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

    private class PlacesTextSearchResponse
    {
        public List<PlaceResult>? Places { get; set; }
    }

    private class PlaceResult
    {
        public PlaceLocation? Location { get; set; }
    }

    private class PlaceLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

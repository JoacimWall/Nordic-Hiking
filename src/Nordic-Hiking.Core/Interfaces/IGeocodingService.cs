namespace NordicHiking.Core.Interfaces;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string placeName, string region, string country);
    Task<(double Latitude, double Longitude)?> GetCoordinatesFromGoogleAsync(string placeName, string region, string country);
}

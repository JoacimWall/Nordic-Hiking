namespace NordicHiking.Core.Interfaces;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string placeName, string region, string country);
}

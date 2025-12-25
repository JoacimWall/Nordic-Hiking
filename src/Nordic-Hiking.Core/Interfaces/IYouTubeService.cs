using NordicHiking.Core.Models;

namespace NordicHiking.Core.Interfaces;

public interface IYouTubeService
{
    Task<(string ChannelId, string ChannelName)> GetChannelInfoAsync(string channelUrl);
    Task<List<Video>> GetVideosFromChannelAsync(string channelId);
    Task<string?> GetVideoTranscriptAsync(string videoId);
}

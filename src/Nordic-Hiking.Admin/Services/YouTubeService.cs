using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;
using NordicHiking.Core.Interfaces;
using NordicHiking.Core.Models;

namespace NordicHiking.Admin.Services;

public class YouTubeService : IYouTubeService
{
    private readonly YoutubeClient _youtube;

    public YouTubeService()
    {
        _youtube = new YoutubeClient();
    }

    public async Task<(string ChannelId, string ChannelName)> GetChannelInfoAsync(string channelUrl)
    {
        // Hantera @handle format (t.ex. https://www.youtube.com/@PeterPersson)
        if (channelUrl.Contains("@"))
        {
            var handle = channelUrl.Split('@').Last().TrimEnd('/');
            var channel = await _youtube.Channels.GetByHandleAsync(handle);
            return (channel.Id.Value, channel.Title);
        }
        else
        {
            var channel = await _youtube.Channels.GetAsync(channelUrl);
            return (channel.Id.Value, channel.Title);
        }
    }

    public async Task<List<Video>> GetVideosFromChannelAsync(string channelId)
    {
        var videos = new List<Video>();

        await foreach (var playlistVideo in _youtube.Channels.GetUploadsAsync(channelId))
        {
            // Hämta fullständig video-metadata
            var videoMetadata = await _youtube.Videos.GetAsync(playlistVideo.Id.Value);

            videos.Add(new Video
            {
                YoutubeVideoId = videoMetadata.Id.Value,
                Title = videoMetadata.Title,
                Description = videoMetadata.Description ?? string.Empty,
                ThumbnailUrl = videoMetadata.Thumbnails.GetWithHighestResolution()?.Url ?? string.Empty,
                PublishedAt = videoMetadata.UploadDate.UtcDateTime,
                IsProcessed = false
            });
        }

        return videos;
    }

    public async Task<string?> GetVideoTranscriptAsync(string videoId)
    {
        try
        {
            var trackManifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            // Prioritera svenska och engelska captions
            var trackInfo = trackManifest.Tracks
                .FirstOrDefault(t => t.Language.Code.StartsWith("sv")) ??
                trackManifest.Tracks
                .FirstOrDefault(t => t.Language.Code.StartsWith("en")) ??
                trackManifest.Tracks.FirstOrDefault();

            if (trackInfo == null)
                return null;

            var track = await _youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
            var transcript = string.Join("\n", track.Captions.Select(c => c.Text));

            return transcript;
        }
        catch
        {
            return null;
        }
    }
}

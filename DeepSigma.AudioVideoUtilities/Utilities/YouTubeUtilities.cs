
using DeepSigma.AudioVideoUtilities.Enums;
using DeepSigma.AudioVideoUtilities.Models;
using System.Net;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using static NReco.VideoInfo.MediaInfo;

namespace DeepSigma.AudioVideoUtilities.Utilities;

public static class YouTubeUtilities
{
    /// <summary>
    /// Downloads a YouTube video based on the specified modality (video, audio, or both).
    /// </summary>
    /// <remarks>
    /// Also, age-restricted videos may still require additional verification steps beyond just setting the consent cookie.
    /// Consider the YtDlp tool for a more robust solution when dealing with such content.
    /// </remarks>
    /// <param name="url"></param>
    /// <param name="save_directory"></param>
    /// <param name="file_name"></param>
    /// <param name="modality"></param>
    /// <param name="cancel_token"></param>
    /// <returns></returns>
    public static async Task<bool> TryGetYouTubeVideoAsync(string url, string save_directory, string file_name, YouTubeDownloadModalityType modality, CancellationToken cancel_token = default)
    {
        YoutubeClient youtube = new(GetHttpClientWithConsentEnabledForAgeRestrictedVideos());

        Video video = await youtube.Videos.GetAsync(url, cancel_token);
        StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id, cancel_token);

        IStreamInfo? streamInfo = GetStreamInfo(streamManifest, modality);

        if (streamInfo is null) return false;

        Directory.CreateDirectory(save_directory); // create directory if it doesn't exist
        await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(save_directory, file_name), cancellationToken: cancel_token);
        return true;
    }

    /// <summary>
    /// Downloads the highest quality audio and video streams from a YouTube video and combines them into a single file.
    /// </summary>
    /// <remarks>
    /// Note: This method requires FFmpeg to be installed and accessible in the system's PATH for combining audio and video streams.
    /// Also, age-restricted videos may still require additional verification steps beyond just setting the consent cookie.
    /// Consider the YtDlp tool for a more robust solution when dealing with such content.
    /// </remarks>
    /// <param name="url"></param>
    /// <param name="save_directory"></param>
    /// <param name="file_name"></param>
    /// <param name="cancel_token"></param>
    /// <returns></returns>
    public static async Task<bool> TryGetYouTubeVideoCombinedToHighestQualityAsync(string url, string save_directory, string file_name, CancellationToken cancel_token = default)
    {

        YoutubeClient youtube = new(GetHttpClientWithConsentEnabledForAgeRestrictedVideos());

        Video video = await youtube.Videos.GetAsync(url, cancel_token);
        StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id, cancel_token);

        IStreamInfo? audio_stream = GetStreamInfo(streamManifest, YouTubeDownloadModalityType.Audio);
        IStreamInfo? video_stream = GetStreamInfo(streamManifest, YouTubeDownloadModalityType.Video);

        if (audio_stream is null || video_stream is null) return false;

        string guid = Guid.NewGuid().ToString();
        string audio_file_name = guid + ".mp3";
        string video_file_name = guid + ".mp4";

        Directory.CreateDirectory(save_directory); // create directory if it doesn't exist
        await youtube.Videos.Streams.DownloadAsync(audio_stream, Path.Combine(save_directory, audio_file_name), cancellationToken: cancel_token);
        await youtube.Videos.Streams.DownloadAsync(video_stream, Path.Combine(save_directory, video_file_name), cancellationToken: cancel_token);
        
        MP4Utilities.CombineAudioAndVideoStreams(Path.Combine(save_directory, audio_file_name), Path.Combine(save_directory, video_file_name), save_directory, file_name);
        return true;
    }

    /// <summary>
    /// Gets an HttpClient with the necessary consent cookie enabled for accessing age-restricted YouTube videos.
    /// Note: This method sets a cookie that indicates user consent for viewing age-restricted content.
    /// However, it does not bypass YouTube's age verification mechanisms currently in place for certain videos.
    /// You may still encounter age-restricted content that requires additional verification steps beyond just setting this cookie.
    /// For a more robust solution, consider using authenticated requests with a logged-in YouTube account using YtDlp or similar tools.
    /// </summary>
    /// <returns></returns>
    public static HttpClient GetHttpClientWithConsentEnabledForAgeRestrictedVideos()
    {
        var handler = new HttpClientHandler { UseCookies = true };

        handler.CookieContainer.Add(
            new Uri("https://www.youtube.com"),
            new Cookie("CONSENT", "YES+cb")
        );

        var http = new HttpClient(handler);

        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0 Safari/537.36");

        return http;
    }

    /// <summary>
    /// Gets YouTube video data from the specified URL.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<VideoData> GetYouTubeVideoData(string url, CancellationToken cancellation = default)
    {
        YoutubeClient youtube = new(GetHttpClientWithConsentEnabledForAgeRestrictedVideos());
        Video video = await youtube.Videos.GetAsync(url, cancellation);
        return GetVideoData(video);
    }

    /// <summary>
    /// Gets the appropriate stream info based on the specified modality.
    /// </summary>
    /// <param name="streamManifest"></param>
    /// <param name="modality"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static IStreamInfo GetStreamInfo(StreamManifest streamManifest, YouTubeDownloadModalityType modality)
    {
        return modality switch
        {
            YouTubeDownloadModalityType.Video => streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality(),
            YouTubeDownloadModalityType.Audio => streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate(),
            YouTubeDownloadModalityType.Both => streamManifest.GetMuxedStreams().GetWithHighestVideoQuality(),
            _ => throw new ArgumentOutOfRangeException(nameof(modality), modality, null),
        };
    }

    /// <summary>
    /// Converts a YoutubeExplode Video object to a VideoData object.
    /// </summary>
    /// <param name="video"></param>
    /// <returns></returns>
    private static VideoData GetVideoData(Video video)
    {
        VideoData videoData = new()
        {
            Author = video.Author.ChannelTitle,
            FileName = video.Title,
            Id = video.Id.Value,
            Duration = video.Duration,
            URL = video.Url,
            UploadDate = video.UploadDate,
            Description = video.Description,
            Keywords = video.Keywords
        };
        return videoData;
    }
}

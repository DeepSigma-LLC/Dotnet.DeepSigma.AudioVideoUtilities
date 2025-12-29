
namespace DeepSigma.AudioVideoUtilities.Models;

/// <summary>
/// Video Data
/// </summary>
public class VideoData
{
    /// <summary>
    /// File Name
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// Author name
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the video
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// URL of the video
    /// </summary>
    public string URL { get; set; } = string.Empty;

    /// <summary>
    /// Video Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Upload Date
    /// </summary>
    public DateTimeOffset UploadDate { get; set; }

    /// <summary>
    /// Description of the video
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Keywords associated with the video
    /// </summary>
    public IReadOnlyList<string> Keywords { get; set; } = [];

    /// <summary>
    /// Frames Per Second of the video
    /// </summary>
    public int? FramesPerSecond { get; set; } = null;
}

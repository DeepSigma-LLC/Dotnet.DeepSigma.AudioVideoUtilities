
using System.Diagnostics.CodeAnalysis;

namespace DeepSigma.AudioVideoUtilities.Models;

/// <summary>
/// Image Data
/// </summary>
public class ImageData
{
    /// <summary>
    /// Path to the image file
    /// </summary>
    public required string ImagePath { get; set; }

    /// <summary>
    /// Time the image was extracted from the video
    /// </summary>
    public required TimeSpan ImageExtractionTime { get; set; }

    /// <summary>
    /// Description of the image
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc cref="ImageData"/>
    public ImageData() { }

    /// <inheritdoc cref="ImageData"/>
    [SetsRequiredMembers]
    public ImageData(string imagePath, TimeSpan imageExtractionTime)
    {
        ImagePath = imagePath;
        ImageExtractionTime = imageExtractionTime;
    }
}

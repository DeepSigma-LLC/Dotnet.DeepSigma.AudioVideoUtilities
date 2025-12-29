using NReco.VideoConverter;
using DeepSigma.AudioVideoUtilities.Models;

namespace DeepSigma.AudioVideoUtilities.Utilities;

/// <summary>
/// Utilities for handling MP4 files
/// </summary>
public static class MP4Utilities
{

    /// <summary>
    /// Converts an MP4 file to a series of images at specified intervals.
    /// </summary>
    /// <remarks>
    /// Note: FFMpeg must be installed and accessible in the system PATH for this method to work.
    /// </remarks>
    /// <param name="SourceFolder"></param>
    /// <param name="InputFileName"></param>
    /// <param name="OutputFolder"></param>
    /// <param name="OutputFileName"></param>
    /// <param name="EndFrameTime"></param>
    /// <param name="SecondsBetweenImages"></param>
    /// <returns></returns>
    public static List<ImageData> MP4ToImages(string SourceFolder, string InputFileName, string OutputFolder, string OutputFileName, TimeSpan EndFrameTime, float SecondsBetweenImages = 5)
    {
        List<ImageData> imageData = [];
        long count = 0;
        float CurrentTimeInSeconds = 0;
        string TempFileName = OutputFileName + "_" + count.ToString() + ".jpeg";
        while (CurrentTimeInSeconds < (float)EndFrameTime.TotalSeconds)
        {
            imageData.Add(new ImageData(Path.Combine(OutputFolder, TempFileName), new TimeSpan(0, 0, (int)CurrentTimeInSeconds)));
            MP4ToSavedImage(SourceFolder, InputFileName, OutputFolder, TempFileName, CurrentTimeInSeconds);
            CurrentTimeInSeconds += SecondsBetweenImages;

            count++;
            TempFileName = OutputFileName + "_" + count.ToString() + ".jpeg";
        }

        imageData.Add(new ImageData(Path.Combine(OutputFolder, TempFileName), new TimeSpan(0, 0, (int)EndFrameTime.TotalSeconds)));
        MP4ToSavedImage(SourceFolder, InputFileName, OutputFolder, TempFileName, (float)EndFrameTime.TotalSeconds);
        return imageData;
    }

    /// <summary>
    /// Converts an MP4 file to a saved image at a specific frame time.
    /// </summary>
    /// <remarks>
    /// Note: FFMpeg must be installed and accessible in the system PATH for this method to work.
    /// </remarks>
    /// <param name="SourceFolder"></param>
    /// <param name="InputFileName"></param>
    /// <param name="OutputFolder"></param>
    /// <param name="OutputFileName"></param>
    /// <param name="FrameTimeInSeconds"></param>
    /// <exception cref="Exception"></exception>
    private static void MP4ToSavedImage(string SourceFolder, string InputFileName, string OutputFolder, string OutputFileName, float FrameTimeInSeconds)
    {
        FFMpegConverter Converter = new();
        string FullInputFilePath = Path.Combine(SourceFolder, InputFileName);
        string FullOutputFilePath = Path.Combine(OutputFolder, OutputFileName);
        try
        {
            Converter.GetVideoThumbnail(FullInputFilePath, FullOutputFilePath, FrameTimeInSeconds);
        }
        catch (Exception ex)
        {
            throw new Exception("Something went wrong while converting from MP4 to images. " + ex.Message);
        }
    }

    /// <summary>
    /// Converts an MP4 audio or video file to an MP3 audio file using the specified input and output file names within
    /// a given directory.
    /// </summary>
    /// <remarks>
    /// Note: FFMpeg must be installed and accessible in the system PATH for this method to work.
    /// </remarks>
    /// <param name="Directory">The path to the directory containing the input MP4 file and where the output MP3 file will be saved.</param>
    /// <param name="InputFile">The name of the input MP4 file to convert. Must be a valid file name within the specified directory.</param>
    /// <param name="OutputFile">The name of the output MP3 file to create. Must be a valid file name.</param>
    /// <exception cref="Exception">Thrown if an error occurs during the conversion process.</exception>
    public static void ConvertMP4ToMP3(string Directory, string InputFile, string OutputFile)
    {
        FFMpegConverter Converter = new();
        string FullInputFilePath = Path.Combine(Directory, InputFile);
        string FullOutputFilePath = Path.Combine(Directory, OutputFile);
        try
        {
            Converter.ConvertMedia(FullInputFilePath, FullOutputFilePath, "mp3");
        }
        catch (Exception ex)
        {
            throw new Exception("Something went wrong while converting from MP4 to MP3. " + ex.Message);
        }
    }

    /// <summary>
    /// Combines audio and video streams into a single MP4 file.
    /// </summary>
    /// <remarks>
    /// Note: FFMpeg must be installed and accessible in the system PATH for this method to work.
    /// </remarks>
    /// <param name="AudioFullFilePath"></param>
    /// <param name="VideoFullFilePath"></param>
    /// <param name="OutputDirectory"></param>
    /// <param name="OutputFileName"></param>
    /// <exception cref="Exception"></exception>
    public static void CombineAudioAndVideoStreams(string AudioFullFilePath, string VideoFullFilePath, string OutputDirectory, string OutputFileName)
    {
        FFMpegConverter Converter = new();
        FFMpegInput[] InputFiles = 
        { 
            new(AudioFullFilePath), 
            new(VideoFullFilePath) 
        };
        string OutputFullFilePath = Path.Combine(OutputDirectory, OutputFileName + ".mp4");

        ConcatSettings settings = new()
        {
            AudioCodec = "copy",
            VideoCodec = "copy",
            CustomOutputArgs = " -map 0:v:0? -map 1:a:0? "
        };

        try
        {
            Converter.ConvertMedia(InputFiles, OutputFullFilePath, null, settings);
        }
        catch (Exception ex)
        {
            throw new Exception("Somthing went wrong while attempting to combine audio and video streams. " + ex.Message);
        }
    }
}

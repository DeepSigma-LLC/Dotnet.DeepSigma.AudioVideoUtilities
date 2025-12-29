using DeepSigma.AudioVideoUtilities.Models;
using DeepSigma.AudioVideoUtilities.Utilities;

namespace DeepSigma.AudioVideoUtilities.AI;

/// <summary>
/// AI Audio Video Controller
/// </summary>
public class AIVideoProcessor
{
    private AIUnstructuredFile DataFile { get; set; } = new AIUnstructuredFile();
    public int ImagePerSecond { get; set; } = 5;
    private string AudioSegmentFolderName { get; } = "AudioSegmentOutput";
    private string ImageSegmentFolderName { get; } = "ImageSegmentOutput";

    Func<string, string> ImageToText { get; init; }
    Func<string, AIUnstructuredPartitionedFile, List<AIUnstructuredTranscriptSegment>> GetAudioTransaction { get; init; }

    public AIVideoProcessor(Func<string, string> image_to_text, Func<string, AIUnstructuredPartitionedFile, List<AIUnstructuredTranscriptSegment>> get_audio_transaction) 
    {
        ImageToText = image_to_text;
        GetAudioTransaction = get_audio_transaction;
    }

    /// <summary>
    /// Get the full transcription text
    /// </summary>
    /// <returns></returns>
    public string GetTranscriptionText() => DataFile.GetAllText();

    /// <summary>
    /// Get the AI unstructured data
    /// </summary>
    /// <returns></returns>
    public AIUnstructuredFile GetAIUnstructuredData() => DataFile;

    /// <summary>
    /// Get all image data
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ImageData> GetAllImageData()
    {
        return DataFile.PartitionedFiles.SelectMany(x => x.Data.SelectMany(t => t.Images));
    }

    /// <summary>
    /// Download YouTube video components
    /// </summary>
    /// <param name="URL"></param>
    /// <param name="targetDirectory"></param>
    /// <param name="FileName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public VideoData DownloadYouTubeVideoComponents(string URL, string targetDirectory, string FileName)
    {
        throw new NotImplementedException();
        //return AudioVideoUtilities.DownloadYouTubeVideosSeperate(URL, targetDirectory, FileName);
    }

    /// <summary>
    /// AI Audio Video Processing
    /// </summary>
    /// <param name="SelectedDirectory"></param>
    /// <param name="SelectedFileName"></param>
    /// <param name="AllowAIImageProcessing"></param>
    public void AiAudioVideoProcessing(string SelectedDirectory, string SelectedFileName, bool AllowAIImageProcessing = false)
    {
        DataFile = new AIUnstructuredFile();
        string AudioOutputDirectory = Path.Combine(SelectedDirectory, AudioSegmentFolderName);
        string ImageOutputDirectory = Path.Combine(SelectedDirectory, ImageSegmentFolderName);
        TempDirectoryCleanup(SelectedDirectory);

        string ExtractedMP3FileName = Path.ChangeExtension(SelectedFileName, "mp3");
        List<ImageData> images = [];
        if (Path.GetExtension(SelectedFileName).Equals(".mp4", StringComparison.InvariantCultureIgnoreCase))
        {
            MP4Utilities.ConvertMP4ToMP3(SelectedDirectory, SelectedFileName, ExtractedMP3FileName);
        }
        MP3Utilities.SplitMP3(SelectedDirectory, ExtractedMP3FileName, AudioOutputDirectory);
        ProcessAudioFiles(AudioOutputDirectory);

        if (Path.GetExtension(SelectedFileName).Equals(".mp4",StringComparison.InvariantCultureIgnoreCase))
        {
            images = ProcessVideoImages(SelectedDirectory, SelectedFileName, ImageOutputDirectory, AllowAIImageProcessing);
        }
        DataFile.AddImages(images);
    }

    /// <summary>
    /// Cleans up temporary directories
    /// </summary>
    /// <param name="SelectedDirectory"></param>
    private void TempDirectoryCleanup(string SelectedDirectory)
    {
        string AudioOutputDirectory = Path.Combine(SelectedDirectory, AudioSegmentFolderName);
        string ImageOutputDirectory = Path.Combine(SelectedDirectory, ImageSegmentFolderName);
        Directory.Delete(AudioOutputDirectory, true);
        Directory.Delete(ImageOutputDirectory, true);
        Directory.CreateDirectory(AudioOutputDirectory);
        Directory.CreateDirectory(ImageOutputDirectory);
    }

    /// <summary>
    /// Process audio files
    /// </summary>
    /// <param name="AudioFileDirectory"></param>
    private void ProcessAudioFiles(string AudioFileDirectory)
    {
        int FileCount = 0;
        foreach (string partitionedMP3 in Directory.GetFiles(AudioFileDirectory).Select(x => Path.GetFileName(x)).ToArray())
        {
            string MP3FilePath = Path.Combine(AudioFileDirectory, partitionedMP3);

            AIUnstructuredPartitionedFile file = new()
            {
                StartTime = DataFile.GetLastFileEndTime() ?? TimeSpan.MinValue,
                FileDirectory = AudioFileDirectory,
                FileName = partitionedMP3
            };
            List<AIUnstructuredTranscriptSegment> Output = GetAudioTransaction(MP3FilePath, file);
            file.Data.AddRange(Output);
            DataFile.PartitionedFiles.Add(file);

            FileCount++;
        }
    }

    /// <summary>
    /// Process video images
    /// </summary>
    /// <param name="FileDirectory"></param>
    /// <param name="SelectedVideoFileName"></param>
    /// <param name="ImageOutputDirectory"></param>
    /// <param name="AllowAIImageProcessing"></param>
    /// <returns></returns>
    private List<ImageData> ProcessVideoImages(string FileDirectory, string SelectedVideoFileName, string ImageOutputDirectory, bool AllowAIImageProcessing)
    {
        TimeSpan? videoEndTime = DataFile.GetLastFileEndTime();
        if (videoEndTime is null) return [];

        List<ImageData> images = MP4Utilities.MP4ToImages(FileDirectory, SelectedVideoFileName, ImageOutputDirectory, "ExtractedImage", videoEndTime.Value, ImagePerSecond);

        if (!AllowAIImageProcessing) return images;
        
        images.ForEach(x => x.Description = ImageToText(x.ImagePath));
        return images;
    }
}

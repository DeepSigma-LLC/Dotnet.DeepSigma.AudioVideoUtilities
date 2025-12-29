using DeepSigma.AudioVideoUtilities.Models;
using System.Text;

namespace DeepSigma.AudioVideoUtilities.AI;

public class AIUnstructuredFile
{
    public List<AIUnstructuredPartitionedFile> PartitionedFiles {  get; set; } = [];
    public AIUnstructuredFile() { }

    public TimeSpan? GetLastFileEndTime()
    {
        return PartitionedFiles.Count == 0 
            ? TimeSpan.Zero 
            : PartitionedFiles.LastOrDefault()?.GetFileEndTime();
    }

    public string GetAllText()
    {
        StringBuilder stringBuilder = new();
        foreach (AIUnstructuredPartitionedFile file in PartitionedFiles)
        {
            stringBuilder.Append(String.Join(" ", file.GetAllText()));
        }
        return stringBuilder.ToString();
    }

    public void AddImages(List<ImageData> images)
    {
        foreach (ImageData image in images)
        {
            TimeSpan imageTime = image.ImageExtractionTime;
            AIUnstructuredPartitionedFile? file = PartitionedFiles.Where(x => imageTime >= x.StartTime && imageTime <= x.GetFileEndTime()).FirstOrDefault();
            if (file is null) continue;

            AIUnstructuredTranscriptSegment? targetSegment = file.Data.Where(x => imageTime >= x.GetTotalStartTime() && imageTime <= x.GetTotalEndTime()).FirstOrDefault();
            targetSegment?.Images.Add(image);
        }
    }
}

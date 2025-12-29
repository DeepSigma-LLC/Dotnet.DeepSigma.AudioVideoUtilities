using System.Text;

namespace DeepSigma.AudioVideoUtilities.AI;

public class AIUnstructuredPartitionedFile()
{
    public required string FileDirectory {  get; set; }
    public required string FileName { get; set; }
    public TimeSpan StartTime { get; set; } = TimeSpan.Zero;
    public List<AIUnstructuredTranscriptSegment> Data { get; set; } = [];

    public TimeSpan GetFileEndTime()
    {
        return Data.Select(x => x.GetTotalEndTime()).Max();
    }

    public string GetAllText()
    {
        StringBuilder stringBuilder = new();
        Data.ForEach(segment => 
        {
            stringBuilder.Append(segment.Text);
            stringBuilder.Append(' ');
        });
        return stringBuilder.ToString();
    }
}

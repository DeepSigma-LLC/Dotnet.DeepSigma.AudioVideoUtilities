using DeepSigma.AudioVideoUtilities.Models;

namespace DeepSigma.AudioVideoUtilities.AI;

public class AIUnstructuredTranscriptSegment()
{
    public string Text {  get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; } = new TimeSpan();
    public TimeSpan EndTime { get; set; } = new TimeSpan();
    public decimal? SentimentScore { get; set; }
    public float[] Embedding { get; set; } = [];
    public decimal NoInformationProbability { get; set; }
    public List<ImageData> Images { get; set; } = [];
    public AIUnstructuredPartitionedFile ParentFile { get; set; }

    public float CosineSimilartyScore(float[] embedding)
    {
        throw new NotImplementedException();
        //return LinearAlgebraUtilities.GetVectorCosineSimilarty(Embedding, embedding);
    }

    public TimeSpan SegmentDuration() => EndTime - StartTime;

    public TimeSpan GetTotalStartTime() => ParentFile.StartTime + StartTime;
    
    public TimeSpan GetTotalEndTime() => ParentFile.StartTime + EndTime;
}

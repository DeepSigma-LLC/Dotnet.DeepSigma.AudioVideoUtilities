using Xunit;
using DeepSigma.AudioVideoUtilities.Utilities;

namespace DeepSigma.AudioVideoUtilities.Test;

public class AudioVideoUtilities_Test
{
    [Fact]
    public async Task Test_YouTubeDownload_Async()
    {
        string url = "https://www.youtube.com/watch?v=GjPIvvxIQME";
        string downloadDirectory = @"C:\Users\brend\Downloads\VideoDownloads";
        string fileName = "test_video.mp4";
        bool found = await YouTubeUtilities.TryGetYouTubeVideoCombinedToHighestQualityAsync(url, downloadDirectory, fileName);
        
        Assert.True(found);
        Assert.True(File.Exists(Path.Combine(downloadDirectory, fileName)));
    }
}

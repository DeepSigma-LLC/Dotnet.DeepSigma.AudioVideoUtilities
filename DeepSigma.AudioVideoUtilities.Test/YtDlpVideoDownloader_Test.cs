using DeepSigma.AudioVideoUtilities.Utilities;
using Xunit;
namespace DeepSigma.AudioVideoUtilities.Test;

public class YtDlpVideoDownloader_Test
{
    [Fact]
    public async Task Test_DownloadVideoAsync()
    {
        string url = "https://www.youtube.com/watch?v=";
        string downloadDirectory = @"C:\Users\brend\Downloads\VideoDownloads";
        string fileName = "test_video.mp4";
        string cookiesPath = @"C:\Users\brend\Downloads\cookies.txt";   
        (bool found, Exception? error) = await YtDlpVideoDownloader.DownloadVideoAsync(url, cookiesPath, Path.Combine(downloadDirectory, fileName));
        Assert.True(found);
        Assert.Null(error);
    }
}

using System.Diagnostics;

namespace DeepSigma.AudioVideoUtilities.Utilities;

/// <summary>
/// YtDlp Video Downloader. Uses yt-dlp to download videos with cookies support.
/// </summary>
internal class YtDlpVideoDownloader
{
    /// <summary>
    /// Downloads a video using yt-dlp with the specified cookies file and output path.
    /// This method runs yt-dlp as an external process and captures its output. 
    /// This is useful for downloading videos that may require authentication or age verification since it allows the use of cookies.
    /// </summary>
    /// <remarks>
    /// In order to export your cookies from your browser, you can use the "Get cookies.txt Locally" Chrome extension.
    /// https://chromewebstore.google.com/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc
    /// Install the extension, go to the website you want to download from, click the extension icon, and export the cookies to a text file.
    /// </remarks>
    /// <param name="Url"></param>
    /// <param name="cookies_txt_path"></param>
    /// <param name="outputPath"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    internal static async Task<(bool, Exception?)> DownloadVideoAsync(string Url, string cookies_txt_path, string outputPath, CancellationToken cancelToken = default)
    {
        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "yt-dlp.exe");
        string args = $"--cookies \"{cookies_txt_path}\" --js-runtime node -f bestvideo+bestaudio \"{Url}\" -o \"{outputPath}\"";

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var proc = new Process { StartInfo = psi };
        proc.Start();

        string stdout = await proc.StandardOutput.ReadToEndAsync(cancelToken);
        string stderr = await proc.StandardError.ReadToEndAsync(cancelToken);

        await proc.WaitForExitAsync(cancelToken);

        return proc.ExitCode != 0 
            ? (false, new Exception($"yt-dlp failed with exit code {proc.ExitCode}: {stderr}"))
            : (true, null);
    }
}

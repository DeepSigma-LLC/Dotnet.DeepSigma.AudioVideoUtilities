using NAudio.Wave;

namespace DeepSigma.AudioVideoUtilities.Utilities;

/// <summary>
/// Utilities for handling MP3 files
/// </summary>
public static class MP3Utilities
{
    /// <summary>
    /// Splits an MP3 file into smaller files based on the specified frame count.
    /// </summary>
    /// <param name="SourceFolder"></param>
    /// <param name="InputFileName"></param>
    /// <param name="OutputFolder"></param>
    /// <param name="FileFrameCount"></param>
    public static void SplitMP3(string SourceFolder, string InputFileName, string OutputFolder, long FileFrameCount = 5000)
    {
        string strMP3OutputFilename = "ExtractedAudio";
        string FullInputFilePath = Path.Combine(SourceFolder, InputFileName);

        using Mp3FileReader reader = new(FullInputFilePath);
        long count = 0;
        Mp3Frame mp3Frame = reader.ReadNextFrame();
        while (mp3Frame != null)
        {
            string FullFileName = strMP3OutputFilename + "_" + count.ToString() + ".mp3";
            string FullOutputFileName = Path.Combine(OutputFolder, FullFileName);
            FileStream _fs = new(FullOutputFileName, FileMode.Create, FileAccess.Write);

            long CurrentFramecount = 1;
            while (CurrentFramecount <= FileFrameCount)
            {
                if (mp3Frame is null)
                {
                    _fs.Close();
                    return;
                }

                _fs.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                CurrentFramecount++;
                mp3Frame = reader.ReadNextFrame();
            }
            count++;
            _fs.Close();
        }
    }

}

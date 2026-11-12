using DeepSigma.AudioVideoUtilities.Models;
using NReco.VideoConverter;

namespace DeepSigma.AudioVideoUtilities.Utilities;

/// <summary>
/// Utilities for image processing and manipulation.
/// </summary>
public static class ImageUtilities
{
    /// <summary>
    /// Extracts images from an MP4 video file at specified intervals and saves them to the output folder.
    /// </summary>
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
        string TempFileName = TempFileName = OutputFileName + "_" + count.ToString() + ".jpeg";
        while (CurrentTimeInSeconds < (float)EndFrameTime.TotalSeconds)
        {
            imageData.Add(new ImageData(Path.Combine(OutputFolder, TempFileName), new TimeSpan(0, 0, (int)CurrentTimeInSeconds)));
            MP4ToSavedImage(SourceFolder, InputFileName, OutputFolder, TempFileName, CurrentTimeInSeconds);
            CurrentTimeInSeconds = CurrentTimeInSeconds + SecondsBetweenImages;

            count++;
            TempFileName = OutputFileName + "_" + count.ToString() + ".jpeg";
        }

        imageData.Add(new ImageData(Path.Combine(OutputFolder, TempFileName), new TimeSpan(0, 0, (int)EndFrameTime.TotalSeconds)));
        MP4ToSavedImage(SourceFolder, InputFileName, OutputFolder, TempFileName, (float)EndFrameTime.TotalSeconds);
        return imageData;
    }

    public decimal GetImageSemanticSimilarity(string ImagePath1, string ImagePath2, IEmbeddingModel model)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Computes a normalized pixel-difference score between two grayscale frames.
    /// </summary>
    /// <remarks>
    /// Each pixel in the first frame is compared with the corresponding pixel in the second frame. 
    /// The absolute differences are averaged and normalized to the range 0.0 to 1.0, where 0.0 means 
    /// the frames are identical and 1.0 means every pixel differs by the maximum possible grayscale amount.
    /// </remarks>
    /// <param name="a">The first grayscale frame.</param>
    /// <param name="b">The second grayscale frame.</param>
    /// <returns>
    /// A normalized grayscale difference score between 0.0 and 1.0.
    /// </returns>
    public static double FrameDiffScore(GrayFrame a, GrayFrame b)
    {
        EnsureSameSize(a, b);

        long sum = 0;
        int count = a.Pixels.Length;

        for (int i = 0; i < count; i++)
        {
            sum += Math.Abs(a.Pixels[i] - b.Pixels[i]);
        }

        return sum / (double)count / 255.0;
    }

    private static void EnsureSameSize(GrayFrame a, GrayFrame b)
    {
        bool same_size = a.Width == b.Width && a.Height == b.Height;
        if (!same_size)
        {
            throw new ArgumentException("Frames must be the same size");
        }
    }

    /// <summary>
    /// Scores the distinctness of two frames using a combination of SSIM.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double SsimChangeScore(GrayFrame a, GrayFrame b)
    {
        EnsureSameSize(a, b);
        int n = a.Pixels.Length;

        if (n < 2)
            return 0;

        double sumA = 0;
        double sumB = 0;

        for (int i = 0; i < n; i++)
        {
            sumA += a.Pixels[i];
            sumB += b.Pixels[i];
        }

        double meanA = sumA / n;
        double meanB = sumB / n;

        double varianceA = 0;
        double varianceB = 0;
        double covariance = 0;

        for (int i = 0; i < n; i++)
        {
            double da = a.Pixels[i] - meanA;
            double db = b.Pixels[i] - meanB;

            varianceA += da * da;
            varianceB += db * db;
            covariance += da * db;
        }

        varianceA /= n - 1;
        varianceB /= n - 1;
        covariance /= n - 1;

        const double c1 = 6.5025;   // (0.01 * 255)^2
        const double c2 = 58.5225;  // (0.03 * 255)^2 

        double numerator =
            (2 * meanA * meanB + c1) *
            (2 * covariance + c2);

        double denominator =
            (meanA * meanA + meanB * meanB + c1) *
            (varianceA + varianceB + c2);

        if (denominator == 0)
            return 0;

        double ssim = numerator / denominator;

        return 1.0 - ssim;
    }

    /// <summary>
    /// Resizes a grayscale frame to the requested dimensions using nearest-neighbor sampling.
    /// Each output pixel is mapped back to the corresponding source pixel and assigned that source value directly, without interpolation or smoothing.
    /// </summary>
    /// <param name="src">The source image.</param>
    /// <param name="targetWidth">The target width.</param>
    /// <param name="targetHeight">The target height.</param>
    /// <returns>The resized image.</returns>
    public static GrayFrame ResizeNearest(GrayFrame src, int targetWidth, int targetHeight)
    {
        byte[] dst = new byte[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            int srcY = y * src.Height / targetHeight;

            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = x * src.Width / targetWidth;
                dst[y * targetWidth + x] = src.Pixels[srcY * src.Width + srcX];
            }
        }

        return new GrayFrame(targetWidth, targetHeight, dst);
    }

    /// <summary>
    /// Resizes a grayscale frame using bilinear interpolation.
    /// </summary>
    /// <remarks>
    /// Each destination pixel is mapped back to a fractional position in the source
    /// frame. The output value is computed by blending the four nearest source pixels
    /// according to the fractional horizontal and vertical offsets. Compared with
    /// nearest-neighbor resizing, bilinear resizing produces smoother and more stable
    /// results, which is useful before comparing frames for visual differences.
    /// </remarks>
    /// <param name="src">The source grayscale frame.</param>
    /// <param name="targetWidth">The desired output width in pixels.</param>
    /// <param name="targetHeight">The desired output height in pixels.</param>
    /// <returns>A new grayscale frame with the requested dimensions.</returns>
    public static GrayFrame ResizeBilinear(GrayFrame src, int targetWidth, int targetHeight)
    {
        byte[] dst = new byte[targetWidth * targetHeight];

        double scaleX = (double)src.Width / targetWidth;
        double scaleY = (double)src.Height / targetHeight;

        for (int y = 0; y < targetHeight; y++)
        {
            double srcY = (y + 0.5) * scaleY - 0.5;
            int y0 = Clamp((int)Math.Floor(srcY), 0, src.Height - 1);
            int y1 = Clamp(y0 + 1, 0, src.Height - 1);
            double fy = srcY - y0;

            for (int x = 0; x < targetWidth; x++)
            {
                double srcX = (x + 0.5) * scaleX - 0.5;
                int x0 = Clamp((int)Math.Floor(srcX), 0, src.Width - 1);
                int x1 = Clamp(x0 + 1, 0, src.Width - 1);
                double fx = srcX - x0;

                double p00 = src.Pixels[y0 * src.Width + x0];
                double p10 = src.Pixels[y0 * src.Width + x1];
                double p01 = src.Pixels[y1 * src.Width + x0];
                double p11 = src.Pixels[y1 * src.Width + x1];

                double top = p00 + (p10 - p00) * fx;
                double bottom = p01 + (p11 - p01) * fx;
                double value = top + (bottom - top) * fy;

                dst[y * targetWidth + x] = (byte)Math.Round(value);
            }
        }
        return new GrayFrame(targetWidth, targetHeight, dst);
    }

    /// <summary>
    /// Applies a 3x3 box blur to a grayscale frame.
    /// </summary>
    /// <remarks>
    /// Each destination pixel is computed as the average of the source pixel and its
    /// valid neighboring pixels within a 3x3 window. Edge and corner pixels use only
    /// neighbors that fall within the image bounds. This reduces small noise,
    /// compression artifacts, and minor pixel-level variation before comparing frames.
    /// </remarks>
    /// <param name="src">The source grayscale frame.</param>
    /// <returns>A new grayscale frame containing the blurred pixel values.</returns>
    public static GrayFrame BoxBlur3x3(GrayFrame src)
    {
        byte[] dst = new byte[src.Width * src.Height]; // rename varible to 

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                int sum = 0;
                int count = 0;

                for (int dy = -1; dy <= 1; dy++)
                {
                    int yy = y + dy;
                    if (yy < 0 || yy >= src.Height)
                        continue;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int xx = x + dx;
                        if (xx < 0 || xx >= src.Width)
                            continue;

                        sum += src.Pixels[yy * src.Width + xx];
                        count++;
                    }
                }

                dst[y * src.Width + x] = (byte)(sum / count);
            }
        }
        return new GrayFrame(src.Width, src.Height, dst);
    }

    /// <summary>
    /// Scores the distinctness of two frames using a combination of pixel-wise absolute difference and a 3x3 box blur.
    /// </summary>
    /// <param name="previous">The previous grayscale frame.</param>
    /// <param name="current">The current grayscale frame.</param>
    /// <returns>A double representing the distinctness score between the two frames.</returns>
    public static double ScoreDistinctness(GrayFrame previous, GrayFrame current)
    {
        using var _ = default(IDisposable); // placeholder if you later pool buffers
        GrayFrame a = BoxBlur3x3(ResizeBilinear(previous, 320, 180));
        GrayFrame b = BoxBlur3x3(ResizeBilinear(current, 320, 180));
        return SsimChangeScore(a, b);
    }

    /// <summary>
    /// Scores the similarity between two grayscale frames using the Structural Similarity Index (SSIM).
    /// </summary>
    /// <param name="previous">The previous grayscale frame.</param>
    /// <param name="current">The current grayscale frame.</param>
    /// <returns>A double representing the similarity score between the two frames.</returns>
    public static double FastScoreDistinctness(GrayFrame previous, GrayFrame current)
    {
        GrayFrame a = ResizeNearest(previous, 160, 90);
        GrayFrame b = ResizeNearest(current, 160, 90);
        return FrameDiffScore(a, b);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static bool IsDistinctTriggered(GrayFrame previousFrame, GrayFrame currentFrame, double trigger_color_change, double trigger_ssim_change)
    {
        double graySsimChange = FastScoreDistinctness(previousFrame, currentFrame);
        double colorChange = null;
        bool isDistinct = graySsimChange > trigger_ssim_change || colorChange > trigger_color_change;
        return isDistinct;
    }

    public static double IsDistinctWeightedScore(GrayFrame previousFrame, GrayFrame currentFrame, double weight = 0.80)
    {
        double graySsimChange = FastScoreDistinctness(previousFrame, currentFrame);
        double rgbColorChange = (previousFrame, currentFrame);
        double score = weight * graySsimChange + (1-weight) * rgbColorChange;
        return score;
    }

    /// <summary>
    /// Computes a normalized RGB difference score between two RGBA image buffers.
    /// </summary>
    /// <remarks>
    /// For each corresponding pixel, this method compares the red, green, and blue channels and 
    /// accumulates the absolute channel differences. The alpha channel is ignored. The result is 
    /// normalized to the range 0.0 to 1.0, where 0.0 means the images are identical in RGB values 
    /// and 1.0 means every compared channel differs by the maximum possible amount.
    /// </remarks>
    /// <param name="rgbaA">The first RGBA image buffer.</param>
    /// <param name="rgbaB">The second RGBA image buffer.</param>
    /// <param name="width">The number of pixels to compare per row.</param>
    /// <param name="height">The number of rows to compare.</param>
    /// <param name="strideA">The number of bytes per row in the first buffer.</param>
    /// <param name="strideB">The number of bytes per row in the second buffer.</param>
    /// <returns>
    /// A normalized RGB difference score between 0.0 and 1.0.
    /// </returns>
    public static double RgbaDiffScore(byte[] rgbaA, byte[] rgbaB, int width, int height, int strideA, int strideB)
    {
        long sum = 0;
        int count = width * height * 3;

        for (int y = 0; y < height; y++)
        {
            int rowA = y * strideA;
            int rowB = y * strideB;

            for (int x = 0; x < width; x++)
            {
                int iA = rowA + x * 4;
                int iB = rowB + x * 4;

                sum += Math.Abs(rgbaA[iA + 0] - rgbaB[iB + 0]); // R
                sum += Math.Abs(rgbaA[iA + 1] - rgbaB[iB + 1]); // G
                sum += Math.Abs(rgbaA[iA + 2] - rgbaB[iB + 2]); // B
            }
        }

        return sum / (double)count / 255.0;
    }

    private static void MP4ToSavedImage(string SourceFolder, string InputFileName, string OutputFolder, string OutputFileName, float FrameTimeInSeconds)
    {
        var Converter = new FFMpegConverter();
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
}

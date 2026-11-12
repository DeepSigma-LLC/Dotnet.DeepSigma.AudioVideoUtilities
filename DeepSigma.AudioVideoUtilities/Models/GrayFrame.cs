namespace DeepSigma.AudioVideoUtilities.Models;

public sealed class GrayFrame
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public GrayFrame(int width, int height, byte[] pixels)
    {
        if (pixels.Length != width * height)
            throw new ArgumentException("Pixel buffer size does not match dimensions.");

        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public static GrayFrame FromRgba(byte[] rgba, int width, int height, int stride)
    {
        byte[] gray = new byte[width * height];

        for (int y = 0; y < height; y++)
        {
            int srcRow = y * stride;
            int dstRow = y * width;

            for (int x = 0; x < width; x++)
            {
                int i = srcRow + x * 4;

                byte r = rgba[i + 0];
                byte g = rgba[i + 1];
                byte b = rgba[i + 2];

                gray[dstRow + x] = ToLuminance(r, g, b);
            }
        }

        return new GrayFrame(width, height, gray);
    }

    public static GrayFrame FromBgra(byte[] bgra, int width, int height, int stride)
    {
        byte[] gray = new byte[width * height];

        for (int y = 0; y < height; y++)
        {
            int srcRow = y * stride;
            int dstRow = y * width;

            for (int x = 0; x < width; x++)
            {
                int i = srcRow + x * 4;

                byte b = bgra[i + 0];
                byte g = bgra[i + 1];
                byte r = bgra[i + 2];

                gray[dstRow + x] = ToLuminance(r, g, b);
            }
        }

        return new GrayFrame(width, height, gray);
    }

    private static byte ToLuminance(byte r, byte g, byte b)
    {
        // Integer approximation of:
        // 0.299 R + 0.587 G + 0.114 B
        return (byte)((77 * r + 150 * g + 29 * b) >> 8);
    }
}

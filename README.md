# DeepSigma.AudioVideoUtilities

A .NET 10 library for practical audio, video, YouTube, and AI-oriented media processing workflows.

It includes helpers for:

- splitting MP3 files into smaller chunks
- extracting still images from MP4 files at time intervals
- converting MP4 to MP3
- combining separate audio and video streams into a single MP4
- downloading YouTube media with either `YoutubeExplode` or `yt-dlp`
- building an AI-ready structure that aligns transcript segments with extracted video frames

## What the library does

The repository is organized around a core library project, `DeepSigma.AudioVideoUtilities`, plus a test project. The core library exposes utilities and models for common media-processing tasks and includes an `AI` namespace for turning media into a more structured representation for downstream analysis.

### Main capabilities

#### MP3 utilities
`MP3Utilities.SplitMP3(...)` reads an MP3 and writes a sequence of smaller MP3 files based on a frame-count threshold.

#### MP4 utilities
`MP4Utilities` provides methods to:

- extract images from a video over time
- save a thumbnail frame at a specific timestamp
- convert MP4 to MP3
- combine separate audio and video streams into a single MP4

#### YouTube utilities
`YouTubeUtilities` provides methods to:

- download YouTube content as video-only, audio-only, or muxed streams
- download highest-quality audio and video separately and then merge them
- fetch YouTube metadata into a `VideoData` model
- create an `HttpClient` configured with a consent cookie for some age-restricted-content scenarios

#### yt-dlp integration
The repository also contains a `yt-dlp.exe` binary and an internal `YtDlpVideoDownloader` wrapper that launches it as a process with cookies support. This is useful for videos that require authentication, cookies, or stricter age-verification handling.

#### AI-oriented processing pipeline
`AIVideoProcessor` orchestrates a higher-level workflow:

1. convert MP4 to MP3 when needed
2. split audio into partitions
3. call a user-supplied transcription function for each audio segment
4. optionally extract video frames over time
5. optionally call a user-supplied image-to-text function
6. align extracted images with transcript segments inside an `AIUnstructuredFile`

This design makes the package a useful foundation for transcription, indexing, enrichment, and multimodal analysis workflows.

## Repository structure

```text
DeepSigma.AudioVideoUtilities/
├─ DeepSigma.AudioVideoUtilities/
│  ├─ AI/
│  ├─ Assets/
│  ├─ Enums/
│  ├─ Models/
│  ├─ Tools/
│  └─ Utilities/
└─ DeepSigma.AudioVideoUtilities.Test/
```

## Key types

### Utilities

- `MP3Utilities`
- `MP4Utilities`
- `YouTubeUtilities`
- `YtDlpVideoDownloader` (internal)

### Models

- `ImageData`
- `VideoData`

### AI data structures

- `AIVideoProcessor`
- `AIUnstructuredFile`
- `AIUnstructuredPartitionedFile`
- `AIUnstructuredTranscriptSegment`

### Enums

- `YouTubeDownloadModalityType`
  - `Video`
  - `Audio`
  - `Both`

## Requirements

### .NET
The project targets **.NET 10.0**.

### External tools
Some functionality depends on external tools being available:

- **FFmpeg** is required for frame extraction, media conversion, and stream combining.
- **yt-dlp** is used for cookie-based downloads through the internal downloader wrapper.
- The downloader code passes `--js-runtime node`, so **Node.js** may also be required in environments where that yt-dlp option is used.

## Library dependencies visible in the code

The implementation directly references these libraries:

- `NAudio`
- `NReco.VideoConverter`
- `YoutubeExplode`

## Getting started

### Clone and build

```bash
git clone https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.AudioVideoUtilities.git
cd Dotnet.DeepSigma.AudioVideoUtilities
dotnet build
```

### Run tests

```bash
dotnet test
```

## Usage examples

### Split an MP3 into smaller chunks

```csharp
using DeepSigma.AudioVideoUtilities.Utilities;

MP3Utilities.SplitMP3(
    SourceFolder: @"C:\media",
    InputFileName: "podcast.mp3",
    OutputFolder: @"C:\media\chunks",
    FileFrameCount: 5000
);
```

### Extract images from an MP4

```csharp
using DeepSigma.AudioVideoUtilities.Models;
using DeepSigma.AudioVideoUtilities.Utilities;

List<ImageData> images = MP4Utilities.MP4ToImages(
    SourceFolder: @"C:\media",
    InputFileName: "video.mp4",
    OutputFolder: @"C:\media\frames",
    OutputFileName: "frame",
    EndFrameTime: TimeSpan.FromMinutes(2),
    SecondsBetweenImages: 5
);
```

### Convert MP4 to MP3

```csharp
using DeepSigma.AudioVideoUtilities.Utilities;

MP4Utilities.ConvertMP4ToMP3(
    Directory: @"C:\media",
    InputFile: "video.mp4",
    OutputFile: "audio.mp3"
);
```

### Download YouTube audio, video, or both

```csharp
using DeepSigma.AudioVideoUtilities.Enums;
using DeepSigma.AudioVideoUtilities.Utilities;

await YouTubeUtilities.TryGetYouTubeVideoAsync(
    url: "https://www.youtube.com/watch?v=VIDEO_ID",
    save_directory: @"C:\downloads",
    file_name: "downloaded.mp4",
    modality: YouTubeDownloadModalityType.Both
);
```

### Download highest-quality separate streams and merge them

```csharp
using DeepSigma.AudioVideoUtilities.Utilities;

await YouTubeUtilities.TryGetYouTubeVideoCombinedToHighestQualityAsync(
    url: "https://www.youtube.com/watch?v=VIDEO_ID",
    save_directory: @"C:\downloads",
    file_name: "best-merged"
);
```

### Download with yt-dlp and cookies

The internal downloader is designed for scenarios where exported browser cookies are needed. The repository comments mention using a browser extension to export `cookies.txt`, then passing that file to the downloader wrapper.

## AI pipeline example

`AIVideoProcessor` is designed to accept two functions from your application:

- an image-to-text function
- an audio-transcription function that returns transcript segments for a partitioned audio file

Example shape:

```csharp
using DeepSigma.AudioVideoUtilities.AI;

var processor = new AIVideoProcessor(
    image_to_text: imagePath =>
    {
        // Call your vision model here
        return $"Description for {Path.GetFileName(imagePath)}";
    },
    get_audio_transaction: (mp3Path, parentFile) =>
    {
        // Call your transcription model/service here
        return new List<AIUnstructuredTranscriptSegment>();
    }
);

processor.AiAudioVideoProcessing(
    SelectedDirectory: @"C:\media",
    SelectedFileName: "meeting.mp4",
    AllowAIImageProcessing: true
);

string transcript = processor.GetTranscriptionText();
var structured = processor.GetAIUnstructuredData();
```

## Notes and caveats

- FFmpeg must be installed and available on your system path for several `MP4Utilities` methods.
- `YouTubeUtilities` uses `YoutubeExplode` and can optionally merge separate streams into a final MP4.
- The yt-dlp workflow is better suited to cookie-based or stricter verification scenarios.
- The current tests use hard-coded local Windows paths and live/external video resources, so they may require environment-specific adjustment before running successfully on another machine or in CI.
- `DownloadYouTubeVideoComponents(...)` in `AIVideoProcessor` is currently not implemented.

## Improving the project

A few improvements that would make this library easier to consume:

- publish and document a NuGet package flow if desired
- add CI-friendly tests that avoid machine-specific paths
- expose yt-dlp functionality through a public abstraction if it is intended to be part of the supported API
- document the expected contract for transcription and image-description delegates in more detail
- add end-to-end examples for AI enrichment workflows

## License

MIT


# Youtube Downloader

A simple .NET 6 application for downloading **.mp3** and **.mp4** application for mass-downloading videos from Youtube. 
Just supply the source file with direct video URL, playlist link or video name (or all at once).

The app uses **async Parallel loop** when downloading the files for maximum efficiency.
```c#
await Parallel.ForEachAsync(videosDetails, async (videoInfo, token) =>
{ 
    //download video
}
```

## Usage:
After cloning the repo and opening the project in visual studio, you need to change the input and output paths to your desired ones.
```c#
var inputPath = "../../../music.txt";
var outputPath = @"../../../downloaded-music";
```

## Demo

![demo image](https://i.ibb.co/QcVShvm/demo.png)


## Used NuGet packages:

 - [YoutubeSearch](https://www.nuget.org/packages/YouTubeSearch)
 - [YoutubeExplode](https://www.nuget.org/packages/YoutubeExplode)


## Inspired by:
[Berat-Dzhevdetov](https://github.com/Berat-Dzhevdetov/yt-download)

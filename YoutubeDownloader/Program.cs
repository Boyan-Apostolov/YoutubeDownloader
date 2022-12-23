
using AngleSharp.Media;
using System.Text;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;
using YoutubeSearch;

Console.WriteLine("Youtube Downloader");
Console.WriteLine("Download by direct URL, playlist URL or video title");

var youtube = new YoutubeClient();

var inputPath = "../../../music.txt";
var outputPath = @"../../../downloaded-music";

await InitiateProgram();

async Task InitiateProgram()
{
    var videosDetails = await GetVideosDetails();
    if (!videosDetails.Any())
    {
        PrintColor("Add direct (or while playlist) URL-s or titles to source file!...Terminating", ConsoleColor.Red);
        Environment.Exit(1);
    };

    string fileType = await GetFileType();

    CheckDirectory(outputPath);

    var failedDownloads = new List<string>();
    var totalCount = videosDetails.Count;
    var count = 0d;

    await Parallel.ForEachAsync(videosDetails, async (videoInfo, token) =>
    {
        try
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoInfo.Value);
            Stream stream;

            var savePath = $"{outputPath}/{videoInfo.Key}.{fileType}";

            PrintColor($"Started Downloading...{videoInfo.Key}", ConsoleColor.DarkGray);
            if (fileType == "mp3")
            {
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                await youtube.Videos.Streams.DownloadAsync(streamInfo, savePath);
            }
            else if (fileType == "mp4")
            {
                var streamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                await youtube.Videos.Streams.DownloadAsync(streamInfo, savePath);
            }
            count++;

            var downloadedPercentage = ((count / totalCount) * 100).ToString("f2");
            PrintColor($"Downloaded {videoInfo.Key} - ({downloadedPercentage}%) - {count}/{totalCount}", ConsoleColor.Green);
        }
        catch (Exception e)
        {
            failedDownloads.Add(videoInfo.Key);

            PrintColor(new string('*', 10), ConsoleColor.Red);
            PrintColor($"Error downloading: {videoInfo.Key}. May be hidden or deleted!", ConsoleColor.Red);
            PrintColor(e.Message, ConsoleColor.Red);
            PrintColor(new string('*', 10), ConsoleColor.Red);
        }
    });

    Console.WriteLine();
    var failedTitles = failedDownloads.Any()
        ? string.Join(", ", failedDownloads)
        : "(none)";

    PrintColor($"Enjoy your music... {count}/{totalCount} ({fileType})files successfully downloaded.", ConsoleColor.Green);
    PrintColor($"Failed downloads: {failedTitles}", ConsoleColor.Red);
}

void PrintColor(string message, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}

async Task<Dictionary<string, string>> GetVideosDetails()
{
    PrintColor($"Reading source file...", ConsoleColor.Yellow);

    var allVideos = new HashSet<string>();
    try
    {
        allVideos = File.ReadAllLines(inputPath).ToHashSet();
    }
    catch (Exception e)
    {
        PrintColor(new string('*', 10), ConsoleColor.Red);
        PrintColor($"Source file cannot be found!...Terminating", ConsoleColor.Red);
        PrintColor(new string('*', 10), ConsoleColor.Red);
        Environment.Exit(1);
    }


    PrintColor($"{allVideos.Count} source item/s found!", ConsoleColor.Yellow);
    PrintColor($"Loading...", ConsoleColor.Yellow);

    var videoDetails = new Dictionary<string, string>();
    foreach (var videoTitle in allVideos)
    {
        var items = new VideoSearch();
        items.encoding = Encoding.UTF8;

        if (videoTitle.Contains("list=")) // Playlist
        {
            var listId = videoTitle
                .Split("list=")[1] //Split to get Id
                .Split("&")[0]; // Split if there are more arguments

            var playlistUrl = $"https://youtube.com/playlist?list={listId}";
            var videosFromPlaylist = await youtube.Playlists.GetVideosAsync(playlistUrl);
            foreach (var videoFromPlaylist in videosFromPlaylist)
            {
                videoDetails.Add(
                    RemoveForbiddenSymbols(videoFromPlaylist.Title),
                    videoFromPlaylist.Url
                    );
            }
        }
        else
        {
            var formattedTitle = RemoveForbiddenSymbols(items.SearchQuery(videoTitle, 1).First().Title);
            var link = items.SearchQuery(videoTitle, 1).First().Url;

            videoDetails.Add(formattedTitle, link);
        }
    }

    return videoDetails;
}

string RemoveForbiddenSymbols(string s) => s.Replace("/", "-").Replace(".", "").Replace("|", "").Replace(":", "");

void CheckDirectory(string path)
{
    PrintColor($"Checking directory...", ConsoleColor.Yellow);
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
    else
    {
        PrintColor("Deleting old files...", ConsoleColor.Red);
        foreach (FileInfo file in new DirectoryInfo(path).EnumerateFiles())
        {
            file.Delete();
        }
    }
}

async Task<string> GetFileType()
{
    Console.Write("Choose preferred file type [mp3/mp4]: ");
    var fileType = Console.ReadLine().ToLower();
    if (fileType == "mp3" || fileType == "mp4")
    {
        PrintColor($"Selected type: .{fileType}", ConsoleColor.Green);
    }
    else
    {
        PrintColor("Invalid file type!...Restarting", ConsoleColor.Red);
        await InitiateProgram();
    }

    return fileType;
}
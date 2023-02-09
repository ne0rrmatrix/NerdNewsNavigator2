// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;

public class FileService
{
    #region Properties
    private List<JsonData> JsonDataList { get; set; } = new();
    private readonly string _targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "Data.json");
    private readonly List<JsonData> _twit = new()
    {
        new JsonData  {Id = 0, Url = "https://feeds.twit.tv/ww_video_hd.xml" },
        new JsonData {Id = 1, Url = "https://feeds.twit.tv/aaa_video_hd.xml" },
        new JsonData {Id = 2,Url = "https://feeds.twit.tv/hom_video_hd.xml" },
        new JsonData {Id = 3,Url = "https://feeds.twit.tv/hop_video_hd.xml" },
        new JsonData {Id = 4,Url = "https://feeds.twit.tv/howin_video_hd.xml" },
        new JsonData {Id = 5,Url = "https://feeds.twit.tv/ipad_video_hd.xml" },
        new JsonData {Id = 6,Url = "https://feeds.twit.tv/mbw_video_hd.xml" },
        new JsonData {Id = 7,Url = "https://feeds.twit.tv/sn_video_hd.xml" },
        new JsonData {Id = 8,Url = "https://feeds.twit.tv/ttg_video_hd.xml" },
        new JsonData {Id = 9,Url = "https://feeds.twit.tv/tnw_video_hd.xml" },
        new JsonData {Id = 10,Url = "https://feeds.twit.tv/twiet_video_hd.xml" },
        new JsonData {Id = 11,Url = "https://feeds.twit.tv/twig_video_hd.xml" },
        new JsonData {Id = 12,Url = "https://feeds.twit.tv/twit_video_hd.xml" },
        new JsonData {Id = 13,Url = "https://feeds.twit.tv/events_video_hd.xml" },
        new JsonData {Id = 14,Url = "https://feeds.twit.tv/specials_video_hd.xml" },
        new JsonData {Id = 15,Url = "https://feeds.twit.tv/bits_video_hd.xml" },
        new JsonData {Id = 16,Url = "https://feeds.twit.tv/throwback_video_large.xml" },
        new JsonData {Id = 17,Url = "https://feeds.twit.tv/leo_video_hd.xml" },
        new JsonData {Id = 18,Url = "https://feeds.twit.tv/ant_video_hd.xml" },
        new JsonData {Id = 19,Url = "https://feeds.twit.tv/jason_video_hd.xml" },
        new JsonData {Id = 20,Url = "https://feeds.twit.tv/mikah_video_hd.xml" },
    };
    #endregion

    public FileService()
    {
        var jsonData = JsonSerializer.Serialize<List<JsonData>>(_twit);
        if (!File.Exists(_targetFile))
        {
            _ = WriteJsonFile(jsonData, _targetFile);
        }
        JsonDataList = ReadJsonFile(_targetFile).Result;
    }
    private static async Task WriteJsonFile(string text, string targetFile)
    {
        try
        {
            Debug.WriteLine("Current file directory is: " + FileSystem.Current.AppDataDirectory);
            using var outputStream = File.OpenWrite(targetFile);
            using var streamWriter = new StreamWriter(outputStream);
            await streamWriter.WriteAsync(text);
            streamWriter.Close();
            Debug.WriteLine($"{targetFile}, File written to disk!");
        }
        catch
        {
            Debug.WriteLine("Failed to write file!");
        }
    }
    private static Task<List<JsonData>> ReadJsonFile(string fileName)
    {
        var targetFileName = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        using var stream = File.OpenRead(targetFileName);
        var result = JsonSerializer.Deserialize<List<JsonData>>(stream);
        return Task.FromResult(result);
    }
    public List<string> GetJsonData()
    {
        List<string> strings = new List<string>();
        foreach (var jsonData in JsonDataList)
        {
            strings.Add(jsonData.Url);
        }
        return strings;
    }
    public async void DeleteJson()
    {
        var jsonData = JsonSerializer.Serialize<List<JsonData>>(_twit);
        JsonDataList.Clear();
        if (File.Exists(_targetFile))
        {
            File.Delete(_targetFile);
        }
        await WriteJsonFile(jsonData, _targetFile);
        JsonDataList = _twit;
    }
}

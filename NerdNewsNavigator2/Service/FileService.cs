// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Windows.Markup;

namespace NerdNewsNavigator2.Service;

public class FileService
{
    #region Properties
    List<JsonData> JsonDataList { get; set; } = new();
    private readonly string fileName = "Data.json";
    private readonly List<JsonData> _twit = new List<JsonData>
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
        System.Diagnostics.Debug.WriteLine("Json Data loaded!");
        _ = WriteJsonFile(jsonData, fileName);
        var data = ReadJsonFile(fileName).Result;
        foreach (var item in data)
        {
            System.Diagnostics.Debug.WriteLine(item.Url);
        }
        
    }
    public async Task WriteJsonFile(string text, string targetFileName)
    {
        try
        {
            // Write the file content to the app data directory  
            System.Diagnostics.Debug.WriteLine("Current file directory is: " + FileSystem.Current.AppDataDirectory);
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            using FileStream outputStream = System.IO.File.OpenWrite(targetFile);
            using StreamWriter streamWriter = new StreamWriter(outputStream);
            await streamWriter.WriteAsync(text);
            streamWriter.Close();
            System.Diagnostics.Debug.WriteLine($"{targetFile}, File written to disk!");
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("Failed to write file!");
        }
    }
    public async Task<List<JsonData>> ReadJsonFile(string fileName)
    {
        string targetFileName = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        using FileStream stream = System.IO.File.OpenRead(targetFileName);
        var data = JsonSerializer.Deserialize<List<JsonData>>(stream);
        stream.Close();
        System.Diagnostics.Debug.WriteLine("Read file success!");
        return data;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public static class FileService
{
    public static void DeleteFile(string url)
    {
        var tempFile = GetFileName(url);
        try
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
                Debug.WriteLine($"Deleted file {tempFile}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    /// <summary>
    /// Get file name from Url <see cref="string"/>
    /// </summary>
    /// <param name="url">A URL <see cref="string"/></param>
    /// <returns>Filename <see cref="string"/> with file extension</returns>
    public static string GetFileName(string url)
    {
        var temp = new Uri(url).LocalPath;
        var filename = System.IO.Path.GetFileName(temp);
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);

    }
}

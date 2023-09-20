﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

public class HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath) : IDisposable
{

    #region Properties
    private HttpClient _httpClient;

    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage, CancellationToken token);
    public event ProgressChangedHandler ProgressChanged;

    public CancellationTokenSource DownloadCancel { get; set; } = null;

    #endregion
    public async Task StartDownload()
    {
        if (DownloadCancel is not null)
        {
            DownloadCancel.Dispose();
            DownloadCancel = null;
            var Cts = new CancellationTokenSource();
            DownloadCancel = Cts;
        }
        else if (DownloadCancel is null)
        {
            var Cts = new CancellationTokenSource();
            DownloadCancel = Cts;
        }
        _httpClient = new HttpClient();
        Connectivity.Current.ConnectivityChanged += GetCurrentConnectivity;
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            await DownloadFileFromHttpResponseMessage(response, DownloadCancel.Token);
        }
        catch
        {
            Debug.WriteLine("Http Client error");
            App.Downloads.Cancelled = true;
            App.Downloads.CancelAll();
        }
        Connectivity.Current.ConnectivityChanged -= GetCurrentConnectivity;
    }

    private void GetCurrentConnectivity(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            Debug.WriteLine("Internet access has been lost.");
            DownloadCancel.Cancel();
            Connectivity.Current.ConnectivityChanged -= GetCurrentConnectivity;
        }
    }

    private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response, CancellationToken token)
    {
        response.EnsureSuccessStatusCode();
        var totalBytes = response.Content.Headers.ContentLength;

        using var contentStream = await response.Content.ReadAsStreamAsync(token);
        await ProcessContentStream(totalBytes, contentStream, token);
    }

    private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream, CancellationToken token)
    {
        var totalBytesRead = 0L;
        var readCount = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        using var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        do
        {
            var bytesRead = await contentStream.ReadAsync(buffer, cancellationToken: token);
            if (bytesRead == 0)
            {
                isMoreToRead = false;
                TriggerProgressChanged(totalDownloadSize, totalBytesRead, token);
                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken: token);

            totalBytesRead += bytesRead;
            readCount += 1;
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling");
                isMoreToRead = false;
            }
            if (readCount % 100 == 0)
            {
                TriggerProgressChanged(totalDownloadSize, totalBytesRead, token);
            }
        }
        while (isMoreToRead);
        if (DownloadCancel is not null)
        {
            DownloadCancel.Cancel();
            DownloadCancel?.Dispose();
            DownloadCancel = null;
        }
    }

    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead, CancellationToken token)
    {
        if (ProgressChanged == null)
        {
            return;
        }

        double? progressPercentage = null;
        if (totalDownloadSize.HasValue)
        {
            progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);
        }

        ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage, token);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _httpClient != null)
        {
            _httpClient.Dispose();
            _httpClient = null;
        }
    }
}

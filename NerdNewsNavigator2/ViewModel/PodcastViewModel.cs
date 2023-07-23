// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that manages displaying <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastViewModel : SharedViewModel, IRecipient<PageMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastViewModel"/> class.
    /// </summary>
    public PodcastViewModel(ILogger<PodcastViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        ThreadPool.QueueUserWorkItem(async (state) => await GetUpdatedPodcasts());
        WeakReferenceMessenger.Default.Register<PageMessage>(this);
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task GotoShowPage(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={encodedUrl}");
    }

    public void Receive(PageMessage message)
    {
        if(message.Value == "true")
        {
            return;
        }
        Title = message.Value;
    }
}
